using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.BLL.DTO.Comments.Request;
using SocialNetwork.BLL.DTO.Comments.Response;
using SocialNetwork.BLL.DTO.Posts.Request;
using SocialNetwork.BLL.DTO.Posts.Response;

namespace SocialNetwork.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    /// <summary>
    /// Repost
    /// </summary>
    /// <remarks>Change post.</remarks>
    /// <param name="postId"></param>        
    [HttpPost]
    [Route("{postId}")]
    public virtual ActionResult<PostResponseDto> PostPostsPostId(
        [FromRoute, Required] uint postId,
        [FromBody, Required] PostRequestDto postRequestDto)
    {
        return Ok(new PostResponseDto());
    }

    /// <summary>
    /// ChangePost
    /// </summary>
    /// <remarks>Change post.</remarks>
    /// <param name="postId"></param>        
    [HttpPut]
    [Route("{postId}")]       
    public virtual ActionResult<PostResponseDto> PutPostsPostId(
        [FromRoute, Required]uint postId,
        [FromBody, Required] PostRequestDto postRequestDto)
    {
        return Ok(new PostResponseDto());
    }

    /// <summary>
    /// DeletePost
    /// </summary>
    /// <remarks>Delete post.</remarks>         
    [HttpDelete]
    [Route("{postId}")]
    public virtual ActionResult<PostResponseDto> DeletePostsPostId([FromRoute, Required] uint postId)
    {
        return Ok(new PostResponseDto());
    }

    /// <summary>
    /// LikePost
    /// </summary>
    /// <remarks>Like post.</remarks>       
    [HttpPost]
    [Route("{postId}/likes")]
    public virtual ActionResult<PostLikeResponse> PostPostsPostIdLikes([FromRoute, Required] uint postId)
    {
        return Ok(new PostLikeResponse());
    }

    /// <summary>
    /// GetAllPostLikes
    /// </summary>
    /// <remarks>Get all post likes using pagination.</remarks>    
    [HttpGet]
    [Route("{postId}/likes")]
    public virtual ActionResult<List<PostLikeResponse>> GetPostsPostIdLikes(
        [FromRoute, Required] string postId,
        [FromQuery, Required] uint limit,
        [FromQuery, Required] uint currCursor)
    {
        return Ok(new List<PostLikeResponse>() { new PostLikeResponse() });
    }

    /// <summary>
    /// UnlikePost
    /// </summary>
    /// <remarks>Unlike post (for like owner).</remarks>        
    [HttpDelete]
    [Route("{postId}/likes")]
    public virtual ActionResult<PostLikeResponse> DeletePostsPostIdLikes([FromRoute][Required] uint postId)
    {
        return Ok(new PostLikeResponse());
    }

    /// <summary>
    /// CommentPost
    /// </summary>
    /// <remarks>Comment post.</remarks>          
    [HttpPost]
    [Route("{postId}/comments")]
    public virtual ActionResult<CommentResponseDto> PostPostsPostIdComments(
        [FromRoute, Required] uint postId,
        [FromBody, Required] CommentRequestDto commentRequestDto)
    {
        return Ok(new CommentResponseDto());
    }

    /// <summary>
    /// GetAllPostComments
    /// </summary>
    /// <remarks>Get all post comments using pagination.</remarks>    
    [HttpGet]
    [Route("{postId}/comments")]
    public virtual ActionResult<List<CommentResponseDto>> GetPostsPostIdComments(
        [FromRoute, Required] uint postId,
        [FromQuery, Required] uint limit,
        [FromQuery, Required] uint currCursor)
    {
        return Ok(new List<CommentResponseDto>() { new CommentResponseDto() });
    }
}