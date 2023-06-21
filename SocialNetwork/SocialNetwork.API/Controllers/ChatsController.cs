using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.BLL.Contracts;
using SocialNetwork.BLL.DTO.Chats.Request;
using SocialNetwork.BLL.DTO.Chats.Response;
using SocialNetwork.BLL.DTO.Messages.Request;
using SocialNetwork.BLL.DTO.Messages.Response;
using SocialNetwork.BLL.Services;
using SocialNetwork.DAL.Contracts;
using SocialNetwork.DAL.Entities.Chats;
using SocialNetwork.DAL.Entities.Users;

namespace SocialNetwork.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatsController : ControllerBase
{
    private readonly IMapper _mapper;

    private readonly IChatService _chatService;
    

    public ChatsController(IMapper mapper, IChatRepository chatRepository, IChatService chatService)
    {
        _mapper = mapper;
        _chatService = chatService;
    }

    /// <summary>
    /// DeleteChat
    /// </summary>
    /// <remarks>Delete chat. For chat admins.</remarks>      
    [HttpDelete]
    [Authorize(Roles = "Admin, User")]
    [Route("{chatId}")]
    public virtual async Task<ActionResult<ChatResponseDto>> DeleteChatsChatId([FromRoute][Required] uint chatId)
    {
        // Доступно только для авторизованных пользователей
        // проверяем существует ли чат с таким id если нет выбрасываем Bad request
        // Если запрос кинул админ соц сети то удалеям без вопросов
        // иначе проверяем является ли пользователь создателем чата если да то удаляем
        // если нет выкидываем accessdenied

        var chat = await _chatService.GetChatById(chatId);

        var isChatExisted = chat != null;

        if (!isChatExisted)
        {
            return BadRequest("Chat with request Id doesn't exist");
        }
        
        var isUserAuthenticated =
            await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        var userRole = isUserAuthenticated.Principal!.Claims.
            FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType)
            ?.Value;

        var userId = int.Parse(isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c =>c.Type == ClaimsIdentity.DefaultNameClaimType)
            ?.Value!);

        if (userRole == UserType.User.ToString())
        {
            var chatOwner = await _chatService.GetChatOwnerByChatId(chatId);
            var isUserChatOwner = chatOwner.User.Id == userId;
            if (!isUserChatOwner)
            {
                return Forbid("You are not chat Owner");
            }
        }

        await _chatService.DeleteChat(chatId);
        
        return Ok(_mapper.Map<ChatResponseDto>(chat));
    }

    /// <summary>
    /// DeleteChatMember
    /// </summary>
    /// <remarks>Delete a member from chat (for chat admins, admins).</remarks>         
    [HttpDelete]
    [Authorize(Roles = "Admin, User")]
    [Route("{chatId}/members/{memberId}")]
    public virtual async Task<ActionResult<ChatMemberResponseDto>> DeleteChatsChatIdMembersParticipantId([FromRoute][Required] uint chatId, [FromRoute][Required] uint toDeleteUserId)
    {
        // Доступно только для авторизованных пользователей
        // проверяем существует ли чат с таким id если нет выбрасываем Bad request
        // Если запрос кинул админ соц сети то удалеям без вопросов
        // иначе проверяем является ли пользователь админом или создателем чата если да то удаляем
        // если нет выкидываем accessdenied
        
        var chat = await _chatService.GetChatById(chatId);

        var isChatExisted = chat != null;

        if (!isChatExisted)
        {
            return BadRequest("Chat with request Id doesn't exist");
        }
        
        var isUserAuthenticated =
            await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        var userRole = isUserAuthenticated.Principal!.Claims.
            FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType)
            ?.Value;

        var userId = uint.Parse(isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c =>c.Type == ClaimsIdentity.DefaultNameClaimType)
            ?.Value!);

        if (userRole == UserType.User.ToString())
        {
            var isUserHaveAdminPermissions = await _chatService.IsUserHaveAdminPermissions(chatId, userId);
            if (!isUserHaveAdminPermissions)
            {
                return Forbid("User hasn't chat admin permissions");
            }
        }

        var deleteChatMember = await _chatService.DeleteChatMember(chatId, toDeleteUserId);

        if (deleteChatMember == null) return NotFound("ChatMemberNotFound");
        
        return Ok(_mapper.Map<ChatMemberResponseDto>(deleteChatMember));
    }

    // TODO: make DTO
    /// <summary>
    /// GetAllChatMedias
    /// </summary>
    /// <remarks>Get all chat medias (for chat members).</remarks>    
    [HttpGet]
    [Authorize(Roles = "Admin, User")]
    [Route("{chatId}/medias")]
    public virtual IActionResult GetChatChatIdMedias([FromRoute][Required] uint chatId, [FromQuery] uint? limit, [FromQuery] uint? nextCursor)
    {
        // TODO ПРИДУМАТЬ УЖЕ НАКОНЕЦ ТО ЧТО ТО С MEDIA!!!!!!!!!
        return Ok($"GetAllChatMedias");
    }

    // TODO: make DTO
    /// <summary>
    /// GetChatInfo
    /// </summary>
    /// <remarks>Get chat information (name, photo). For chat members.</remarks>         
    [HttpGet]
    [Authorize(Roles = "Admin, User")]
    [Route("{chatId}")]
    public virtual IActionResult GetChatsChatId([FromRoute][Required] uint chatId)
    {
        // TODO ПРИДУМАТЬ УЖЕ НАКОНЕЦ ТО ЧТО ТО С MEDIA для фото чата!!!!!!!!!

        return Ok($"GetChatInfo");
    }

    /// <summary>
    /// GetAllChatMembers
    /// </summary>
    /// <remarks>Get all chat members using pagination (for chat members).</remarks>    
    [HttpGet]
    [Authorize(Roles = "Admin, User")]
    [Route("{chatId}/members")]
    public virtual async Task<ActionResult<List<ChatMemberResponseDto>>> GetChatsChatIdMembers(
        [FromRoute] [Required] uint chatId, [FromQuery][Required] int limit, [FromQuery] int nextCursor)
    {

        // проверяем существует ли такой chatid если нет кидаем Badrequest
        // если запрос кинул админ соц сети то обрабатываем его проверяя параметры пагинации
        // иначе проверяем является ли пользователь участником чата если нет accessdenied
        var chat = await _chatService.GetChatById(chatId);

        var isChatExisted = chat != null;

        if (!isChatExisted)
        {
            return BadRequest("Chat with request Id doesn't exist");
        }

        var isUserAuthenticated =
            await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var userRole = isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType)
            ?.Value;

        var userId = uint.Parse(isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)
            ?.Value!);

        if (userRole == UserType.User.ToString())
        {
            var isUserChatMember = await _chatService.IsUserChatMember(chatId, userId);
            if (!isUserChatMember)
            {
                return Forbid("User isn't chat member");
            }
        }

        var chatMembers = await _chatService.GetAllChatMembers(chatId, limit, nextCursor);

        return Ok(chatMembers.Select(cm => _mapper.Map<ChatMemberResponseDto>(cm)));
    }

    /// <summary>
    /// GetAllChatMessages
    /// </summary>
    /// <remarks>Get all chat messages using pagination (for chat members). </remarks>    
    [HttpGet]
    [Authorize(Roles = "Admin, User")]
    [Route("{chatId}/messages")]
    public virtual async Task<ActionResult<List<MessageResponseDto>>> GetChatsChatIdMessages([FromRoute][Required] uint chatId, [FromQuery][Required()] uint? limit, [FromQuery] uint? nextCursor)
    {
        // проверяем наличеи чата с заданным id 
        // проверяем роль пользователя если админ выполняем запрос 
        // если юзер то проверям является ли он участником чата
        var chat = await _chatService.GetChatById(chatId);

        var isChatExisted = chat != null;

        if (!isChatExisted)
        {
            return BadRequest("Chat with request Id doesn't exist");
        }

        var isUserAuthenticated =
            await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var userRole = isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType)
            ?.Value;

        var userId = uint.Parse(isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)
            ?.Value!);

        if (userRole == UserType.User.ToString())
        {
            var isUserChatMember = await _chatService.IsUserChatMember(chatId, userId);
            if (!isUserChatMember)
            {
                return Forbid("User isn't chat member");
            }
        }

        var messages = await _chatService.GetAllChatMessages(chatId, limit, nextCursor);


        return Ok(messages.Select(m => _mapper.Map<MessageResponseDto>(m)));
    }

    /// <summary>
    /// ChangeChatInfo
    /// </summary>
    /// <remarks>Change chat information (name, photo). For chat admins.</remarks>        
    [HttpPatch]
    [Route("{chatId}")]
    public async virtual Task<ActionResult<ChatResponseDto>> PatchChatsChatId([FromRoute][Required] uint chatId, [FromBody][Required] JsonPatchDocument chatRequestDto)
    {
        return Ok();
    }

    /// <summary>
    /// ChangeChatInfo
    /// </summary>
    /// <remarks>Change chat information (name, photo). For chat admins.</remarks>        
    [HttpPut]
    [Route("{chatId}")]
    public virtual async Task<ActionResult<ChatResponseDto>> PutChatsChatId([FromRoute][Required] uint chatId, [FromBody][Required] ChatRequestDto chatRequestDto)
    {
        var chats = await _chatRepository.SelectAsync((chat) => chat.Id == chatId);
        if (chats.Count == 0)
        {
            return NotFound();
        }

        var existingChat = chats.First();
        _mapper.Map(chatRequestDto, existingChat);

        _chatRepository.Update(existingChat);        

        await _chatRepository.SaveAsync();

        return Ok(_mapper.Map<ChatResponseDto>(existingChat));
    }

    /// <summary>
    /// ChangeMemberStatus
    /// </summary>
    /// <remarks>Updates information about chat member (for chat admins, admins).</remarks>    
    [HttpPatch]
    [Route("{chatId}/members/{memberId}")]
    public async virtual Task<ActionResult<ChatMemberResponseDto>> PatchChatsChatIdMembersMemberId([FromRoute][Required] uint chatId, [FromRoute][Required] uint memberId, [FromBody][Required] ChatRequestDto chatMemberStatusDto)
    {
        return Ok();
    }

    /// <summary>
    /// CreateChat
    /// </summary>
    /// <remarks>Create new chat</remarks>        
    [HttpPost]
    [Authorize(Roles = "Admin, User")]
    public virtual async Task<ActionResult<ChatResponseDto>> PostChats([FromBody][Required] ChatRequestDto chatRequestDto)
    {
        var isUserAuthenticated =
            await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        var userId = uint.Parse(isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)
            ?.Value!);
        
        var newChat = new Chat
        {
            Name = chatRequestDto.Name,
            CreatedAt = DateTime.Now
        };

        var addedChat = await _chatService.AddChat(newChat);

        var chatOwner = new ChatMember
        {
            ChatId = addedChat.Id,
            CreatedAt = DateTime.Now,
            TypeId = ChatMemberType.Owner,
            UserId = userId
        };

        var addedOwner = await _chatService.AddChatMember(chatOwner);

        return Ok(_mapper.Map<ChatResponseDto>(addedChat));

    }

    /// <summary>
    /// AddChatMember
    /// </summary>
    /// <remarks>Add new member to chat (for chat members).</remarks>        
    [HttpPost]
    [Route("{chatId}/members")]
    public virtual ActionResult<ChatMemberResponseDto> PostChatsChatIdMembers([FromRoute][Required] uint chatId, [FromBody][Required] ChatMemberRequestDto postChatMemberDto)
    {
        return new ChatMemberResponseDto();
    }

    /// <summary>
    /// SendMessage
    /// </summary>
    /// <remarks>Send a message to chat.</remarks>     
    [HttpPost]
    [Route("{chatId}/messages")]
    public virtual ActionResult<MessageResponseDto> PostChatsChatIdMessages([FromRoute][Required] uint chatId, [FromBody][Required] MessageRequestDto postChatMemberDto)
    {
        return new MessageResponseDto();
    }
}