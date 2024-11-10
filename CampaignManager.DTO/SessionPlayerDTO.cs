﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CampaignManager.Persistence.Models;

namespace CampaignManager.DTO
{
    public class SessionPlayerDTO
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public string SessonPlayerRole { get; set; } = string.Empty;


        //Natural conversion between SessionPlayer and SessionPlayerDTO
        public static explicit operator SessionPlayer(SessionPlayerDTO sessionPlayerDTO) => new SessionPlayer
        {
            Id = sessionPlayerDTO.Id,
            SessionId = sessionPlayerDTO.SessionId,
            ApplicationUserId = sessionPlayerDTO.ApplicationUserId,
            SessonPlayerRole = sessionPlayerDTO.SessonPlayerRole
        };

        public static explicit operator SessionPlayerDTO(SessionPlayer sessionPlayer) => new SessionPlayerDTO
        {
            Id = sessionPlayer.Id,
            SessionId = sessionPlayer.SessionId,
            ApplicationUserId = sessionPlayer.ApplicationUserId,
            SessonPlayerRole = sessionPlayer.SessonPlayerRole
        };

        /*
        public static SessionPlayer ToSessionPlayer(SessionPlayerDTO sessionPlayerDTO)
        {
            return new SessionPlayer
            {
                Id = sessionPlayerDTO.Id,
                SessionId = sessionPlayerDTO.SessionId,
                ApplicationUserId = string.Empty,
                SessonPlayerRole = sessionPlayerDTO.SessonPlayerRole
            };
        }

        public static SessionPlayerDTO FromSessionPlayer(SessionPlayer sessionPlayer)
        {
            return new SessionPlayerDTO
            {
                Id = sessionPlayer.Id,
                SessionId = sessionPlayer.SessionId,
                ApplicationUserId = string.Empty,
                SessonPlayerRole = sessionPlayer.SessonPlayerRole
            };
        } */

    }
}