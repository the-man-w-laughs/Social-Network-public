﻿using SocialNetwork.DAL.Entities.Chats;
using SocialNetwork.DAL.Entities.Enums;
using SocialNetwork.DAL.Entities.Messages;
using SocialNetwork.DAL.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.BLL.DTO.ChatDto.Response
{
    public class PostChatMemberResponseDto
    {        
        public uint Id { get; set; }
        public ChatMemberType TypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public uint UserId { get; set; }
        public uint ChatId { get; set; }
    }
}
