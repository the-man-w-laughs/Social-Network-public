using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.BLL.Contracts;
using SocialNetwork.BLL.DTO.Chats.Response;
using SocialNetwork.BLL.DTO.Communities.Request;
using SocialNetwork.BLL.DTO.Communities.Response;
using SocialNetwork.BLL.DTO.Medias.Response;
using SocialNetwork.BLL.DTO.Posts.Request;
using SocialNetwork.BLL.DTO.Posts.Response;
using SocialNetwork.BLL.DTO.Users.Request;
using SocialNetwork.BLL.DTO.Users.Response;
using SocialNetwork.BLL.Services;
using SocialNetwork.DAL.Entities.Communities;
using SocialNetwork.DAL.Entities.Posts;
using SocialNetwork.DAL.Entities.Users;

namespace SocialNetwork.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IMapper _mapper;
    private readonly IMediaService _mediaService;
    private readonly IFileService _fileService;
    private readonly IUserService _userService;
    private readonly IPostService _postService;


    public UsersController(
        IMapper mapper,
        IMediaService mediaService,
        IUserService userService,
        IPostService postService,
        IFileService fileService,
        IWebHostEnvironment webHostEnvironment)
    {
        _mapper = mapper;
        _userService = userService;
        _postService = postService;
        _mediaService = mediaService;
        _fileService = fileService;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// GetAllUsers
    /// </summary>
    /// <remarks>Returns all users using pagination.</remarks>
    [HttpGet]
    [Authorize(Roles = "User")]
    public virtual async Task<ActionResult<List<UserResponseDto>>> GetUsers(
        [FromQuery, Required] int limit,
        [FromQuery, Required] int currCursor)
    {
        var users = await _userService.GetUsers(limit, currCursor);

        return Ok(users.Select(up => _mapper.Map<UserResponseDto>(up)));
    }

    /// <summary>
    /// GetAllUserChats
    /// </summary>
    /// <remarks>Get all users chats using pagination (for account owner).</remarks>
    [HttpGet]
    [Route("chats")]
    public virtual async Task<ActionResult<List<ChatResponseDto>>> GetUsersUserIdChats(        
        [FromQuery, Required] int limit,
        [FromQuery] int nextCursor)
    {
        var isUserAuthenticated =
            await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var claimUserId = uint.Parse(isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value!);

        var userChats = await _userService.GetUserChats(claimUserId, limit, nextCursor);

        return Ok(userChats.Select(uc => _mapper.Map<ChatResponseDto>(uc)));
    }

    /// <summary>
    /// GetAllUserCommunities
    /// </summary>
    /// <remarks>Get user's communities using pagination.</remarks>
    [HttpGet]
    [Route("{userId}/communities")]
    public virtual async Task<ActionResult<List<CommunityResponseDto>>> GetUsersUserIdCommunities(
        [FromRoute, Required] uint userId,
        [FromQuery, Required] int limit,
        [FromQuery] int nextCursor)
    {
        var userCommunities = await _userService.GetUserCommunities(userId, limit, nextCursor);

        return Ok(userCommunities.Select(c => _mapper.Map<CommunityResponseDto>(c)));
    }

    /// <summary>
    /// GetAllManagedCommunities
    /// </summary>
    /// <remarks>Get user's communities where user is admin or owner using pagination.</remarks>
    [HttpGet]
    [Route("{userId}/communities/managed")]
    public virtual async Task<ActionResult<List<CommunityResponseDto>>> GetUsersUserIdCommunitiesManaged(
        [FromRoute, Required] uint userId,
        [FromQuery, Required] int limit,
        [FromQuery] int nextCursor)
    {
        var userCommunities = await _userService.GetUserCommunities(userId, limit, nextCursor);

        return Ok(userCommunities.Select(c => _mapper.Map<CommunityResponseDto>(c)));
    }

    /// <summary>
    /// GetAllUserFriends
    /// </summary>
    /// <remarks>Get all user's friends using pagination.</remarks>
    [HttpGet]
    [Route("{userId}/friends")]
    public virtual async Task<ActionResult<List<UserResponseDto>>> GetUsersUserIdFriends(
        [FromRoute, Required] uint userId,
        [FromQuery, Required] int limit,
        [FromQuery] int nextCursor)
    {
        var userFriends = await _userService.GetUserFriends(userId, limit, nextCursor);

        return Ok(userFriends.Select(user => _mapper.Map<UserResponseDto>(user)));
    }
        
    /// <summary>
    /// GetUserProfile
    /// </summary>
    /// <remarks>Get user's profile.</remarks>
    [HttpGet]
    [Route("{userId}/profile")]
    public virtual async Task<ActionResult<UserProfileResponseDto>> GetUsersUserIdProfile([FromRoute, Required] uint userId)
    {
        var userProfile = await _userService.GetUserProfile(userId);

        return Ok(_mapper.Map<UserProfileResponseDto>(userProfile));
    }

    /// <summary>
    /// ChangeUserActivityFields
    /// </summary>
    /// <remarks>Makes user's account deactivated (for account owner or admin).</remarks>
    [HttpPut]
    [Route("{userId}/activity")]
    public virtual ActionResult<UserActivityResponseDto> PutUsersUserId(
        [FromRoute, Required] uint userId,
        [FromBody, Required] UserActivityRequestDto userActivityRequestDto)
    {
        var user = new User { LastActiveAt = DateTime.Now };

        return Ok(_mapper.Map<UserActivityResponseDto>(user));
    }

    /// <summary>
    /// ChangeUserLogin
    /// </summary>
    /// <remarks>Change Login.</remarks>
    [HttpPut]
    [Route("login")]
    public virtual async Task<ActionResult<UserLoginResponseDto>> PutUsersUserIdLogin(
        [FromBody, Required] UserChangeLoginRequestDto userChangeLoginRequestDto)
    {
        /*var users = await _userRepository.GetAllAsync(user => user.Id == userId);
        if (users.Count == 0) return NotFound("User with this ID isn't found");

        var existingUser = users.First();
        _mapper.Map(userChangeLoginRequestDto, existingUser);

        _userRepository.Update(existingUser);

        await _userRepository.SaveAsync();

        return Ok(_mapper.Map<UserLoginResponseDto>(existingUser));*/
        return Ok();
    }

    /// <summary>
    /// ChangeUserPassword
    /// </summary>
    /// <remarks>Change Password.</remarks>
    [HttpPut]
    [Route("password")]
    public virtual ActionResult<UserPasswordResponseDto> PutUsersUserIdPassword(
        [FromBody, Required] UserChangeLoginRequestDto userChangeLoginRequestDto)
    {
        var user = new User { PasswordHash = new byte[] { 12, 228, 123 } };

        return Ok(_mapper.Map<UserPasswordResponseDto>(user));
    }

    /// <summary>
    /// ChangeUserEmail
    /// </summary>
    /// <remarks>Change user email.</remarks>
    [HttpPut]
    [Route("email")]
    public virtual ActionResult<UserEmailResponseDto> PutUsersUserIdProfile(
        [FromBody, Required] UserEmailRequestDto userLoginRequestDto)
    {
        var user = new User { Email = "TestEmail@gmail.com" };

        return Ok(_mapper.Map<UserEmailResponseDto>(user));
    }

    /// <summary>
    /// ChangeUserProfile
    /// </summary>
    /// <remarks>Change user profile(status, sex).</remarks>
    [HttpPatch]
    [Authorize(Roles = "User")]
    [Route("profile")]
    public async virtual Task<ActionResult<UserProfileResponseDto>> PutUsersUserIdProfile(        
        [FromBody, Required] UserProfilePatchRequestDto userProfilePatchRequestDto)
    {
        var isUserAuthenticated =
            await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var userRole = isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType)?.Value;
        var claimUserId = uint.Parse(isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value!);

        try
        {
            var updatedUserProfile = await _userService.ChangeUserProfile(claimUserId, userProfilePatchRequestDto);
            return Ok(updatedUserProfile);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// CreateUserPost
    /// </summary>
    /// <remarks>Create user's post.</remarks>
    [HttpPost]
    [Route("posts")]
    public virtual async Task<ActionResult<UserProfilePostResponseDto>> PostUsersUserIdPosts(        
        [FromBody, Required] PostRequestDto postRequestDto)
    {
        var newUserPost = new Post
        {
            Content = postRequestDto.Content,
            CreatedAt = DateTime.Now
        };

        var isUserAuthenticated =
await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var claimUserId = uint.Parse(isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value!);


        var userProfilePost = await _postService.CreateUserProfilePost(claimUserId, newUserPost);

        return Ok(_mapper.Map<UserProfilePostResponseDto>(userProfilePost));
    }

    /// <summary>
    /// GetAllUserPosts
    /// </summary>
    /// <remarks>Get all user's posts using pagination.</remarks>
    [HttpGet]
    [Route("{userId}/posts")]
    public virtual async Task<ActionResult<List<PostResponseDto>>> GetUsersUserIdPosts(
        [FromRoute, Required] uint userId,
        [FromQuery, Required] int limit,
        [FromQuery, Required] int currCursor)
    {
        var userPosts = await _userService.GetUserPosts(userId, limit, currCursor);

        return Ok(userPosts.Select(up => _mapper.Map<PostResponseDto>(up)));
    }

    /// <summary>
    /// CreateUserMedia
    /// </summary>
    /// <remarks>Create user media.</remarks>    
    [HttpPost]
    [Authorize(Roles = "User")]
    [Route("medias")]
    public async virtual Task<ActionResult<List<MediaResponseDto>>> PostUsersUserIdMedias([Required] List<IFormFile> files)
    {
        var isUserAuthenticated =
    await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var claimUserId = uint.Parse(isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value!);

        var userRole = isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType)?.Value;

        if (files == null)
        {
            return BadRequest();
        }

        var directoryPath = Path.Combine(_webHostEnvironment.ContentRootPath, "UploadedFiles");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        List<MediaResponseDto> medias = new List<MediaResponseDto>();

        foreach (var file in files)
        {
            var filePath = Path.Combine(directoryPath, file.FileName);
            var modifiedFilePath = _fileService.ModifyFilePath(filePath);
            await using (var stream = new FileStream(modifiedFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            medias.Add(await _mediaService.AddUserMedia(modifiedFilePath, claimUserId, file.FileName));
        }

        return Ok(medias);
    }

    /// <summary>
    /// GetAllUserMedias
    /// </summary>
    /// <remarks>Get all user's posts using pagination. (only for user or admin)</remarks>
    [HttpGet]
    [Authorize(Roles = "User")]
    [Route("medias")]
    public async virtual Task<ActionResult<List<MediaResponseDto>>> GetUsersUserIdMedias(        
        [FromQuery, Required] int limit,
        [FromQuery] int currCursor)
    {
        var isUserAuthenticated =
await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var claimUserId = uint.Parse(isUserAuthenticated.Principal!.Claims
            .FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value!);

        var result = await _mediaService.GetUserMediaList(claimUserId, limit, currCursor);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin, User")]
    [Route("{userId}/friends")]
    public virtual async Task<ActionResult<UserProfileResponseDto>> PostUserFriends(
        [FromRoute, Required] uint userId,
        UserFriendRequestDto userFriendRequestDto)
    {
        var user = await _userService.GetUserById(userId);

        var isUserExist = user != null;
        
        if (!isUserExist)
        {
            return BadRequest("Request user doesn't exist");
        }
        
        var friend = await _userService.GetUserById(userFriendRequestDto.Id);

        var isFriendExist = friend != null;

        if (!isFriendExist)
        {
            return BadRequest("Friend with this id doesn't exist");
        }

        if (userId == userFriendRequestDto.Id)
        {
            return BadRequest("Can't add yourself to friends");
        }

        var isUserYourFriend = await _userService.IsUserYourFriend(userId, userFriendRequestDto.Id);

        if (isUserYourFriend)
        {
            return BadRequest("User is already your friend");
        }
        
        var isUserYourFollower = await _userService.IsUserYourFollower(userId, userFriendRequestDto.Id);

        if (isUserYourFollower)
        {
            await _userService.DeleteFollower(userId, userFriendRequestDto.Id);
            var addedFriend = await _userService.AddFriend(userId, userFriendRequestDto.Id);
            return Ok(_mapper.Map<UserProfileResponseDto>(addedFriend.User2.UserProfile));
        }

        var addedFollower = await _userService.Follow(userId, userFriendRequestDto.Id);

        return Ok(_mapper.Map<UserProfileResponseDto>(addedFollower.Source.UserProfile));
    }

    [HttpDelete]
    [Authorize(Roles = "Admin, User")]
    [Route("{userId}/friends")]
    public virtual async Task<ActionResult<UserProfileResponseDto>> DeleteUserFriends(
        [FromRoute, Required] uint userId,
        UserFriendRequestDto userFriendRequestDto)
    {
        var user = await _userService.GetUserById(userId);

        var isUserExist = user != null;
        
        if (!isUserExist)
        {
            return BadRequest("Request user doesn't exist");
        }
        
        var exFriend = await _userService.GetUserById(userFriendRequestDto.Id);

        var isExFriendExist = exFriend != null;

        if (!isExFriendExist)
        {
            return BadRequest("Friend with this id doesn't exist");
        }

        var isUserYourFriend = await _userService.IsUserYourFriend(userId, userFriendRequestDto.Id);

        if (!isUserYourFriend)
        {
            return BadRequest("User is not your Friend");
        }

        await _userService.DeleteFriendship(userId, userFriendRequestDto.Id);

        var follower = await _userService.Follow(userFriendRequestDto.Id, userId);

        return Ok(_mapper.Map<UserProfileResponseDto>(follower.Source.UserProfile));
        
    }

    [HttpDelete]
    [Authorize(Roles = "Admin, User")]
    [Route("{userId}/followers")]
    public virtual async Task<ActionResult<UserProfileResponseDto>> DeleteUserFollowers(
        [FromRoute, Required] uint userId,
        UserFriendRequestDto userFriendRequestDto)
    {
        var user = await _userService.GetUserById(userId);

        var isUserExist = user != null;
        
        if (!isUserExist)
        {
            return BadRequest("Request user doesn't exist");
        }
        
        var exFriend = await _userService.GetUserById(userFriendRequestDto.Id);

        var isExFriendExist = exFriend != null;

        if (!isExFriendExist)
        {
            return BadRequest("Follower with this id doesn't exist");
        }

        var isUserYourFollower = await _userService.IsUserYourFollower(userId, userFriendRequestDto.Id);

        if (!isUserYourFollower)
        {
            return BadRequest("User is not your Follower");
        }

        var deletedFollower = await _userService.DeleteFollower(userId, userFriendRequestDto.Id);

        return Ok(_mapper.Map<UserProfileResponseDto>(deletedFollower.Source.UserProfile));
    }

    [HttpGet]
    [Authorize(Roles = "Admin, User")]
    [Route("{userId}/followers")]
    public virtual async Task<ActionResult<List<UserResponseDto>>> GetUserFollowers(
        [FromRoute, Required] uint userId,
        [FromQuery, Required] int limit,
        [FromQuery] int currCursor)
    {
        var userFollowers = await _userService.GetUserFollowers(userId, limit, currCursor);

        return Ok(userFollowers.Select(followers => _mapper.Map<UserResponseDto>(followers)));
    }
}