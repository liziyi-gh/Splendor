﻿using System;
using Players;
using MsgStruct;
using Gems;
using JsonClasses;
using System.Collections.Generic;
using CardLevelTypes;
using System.Linq;
using Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GameRooms
{

    public class CardPosition
    {
        public string cardLevel;
        public int cardIndex;
    }

    public static class GameRoom
    {
        public static int players_number;
        public static ulong[] players_sequence;
        public static Dictionary<string, int[]> cards_info;
        public static Dictionary<string, int> gems_last_num, cards_last_num;
        public static List<Player> players;
        public static bool reInit = false;
        public static JArray jsonAllCardMsg = new JArray();

        public static void GameRoomInit(JsonRoom msg)
        {
            players_number = msg.players_number;
            players_sequence = msg.players_sequence;
            int gems_num = 7;
            if (players_number == 3) gems_num = 5;
            if (players_number == 2) gems_num = 4;
            gems_last_num = new Dictionary<string, int>
            {
                { GEM.DIAMOND, gems_num },
                { GEM.EMERALD, gems_num },
                { GEM.OBSIDIAN, gems_num },
                { GEM.SAPPHIRE, gems_num },
                { GEM.RUBY, gems_num },
                { GEM.GOLDEN, 5 }
            };
            cards_last_num = new Dictionary<string, int>
            {
                {CardLevelType.levelOneCards, 40 },
                {CardLevelType.levelTwoCards, 30 },
                {CardLevelType.levelThreeCards, 20 },
                {CardLevelType.nobles, players_number+1 }
            };
            cards_info = new Dictionary<string, int[]>
            {
                {CardLevelType.levelOneCards, msg.levelOneCards_info },
                {CardLevelType.levelTwoCards, msg.levelTwoCards_info },
                {CardLevelType.levelThreeCards, msg.levelThreeCards_info },
                {CardLevelType.nobles, msg.nobles_info }
            };
            players = new List<Player>();
            for (int i = 0; i < players_number; i++)
            {
                players.Add(new Player());
                players[i].id = players_sequence[i];
            }
            reInit = true;
        }

        public static void LoadCardMsgJsonFile()
        {
            string jsonStr = Resources.Load<TextAsset>("card_configuration").text;
            jsonAllCardMsg = (JArray)JsonConvert.DeserializeObject(jsonStr);
        }

        public static Player GetPlayer(ulong player_id)
        {
            return players[Array.IndexOf(players_sequence, player_id)];
        }

        public static CardPosition GetCardPosition(int card_id)
        {
            CardPosition cardPos = new CardPosition();

            foreach (var i in cards_info.Keys.ToArray<string>())
                foreach (var j in cards_info[i])
                    if (j == card_id)
                    {
                        cardPos.cardLevel = i;
                        cardPos.cardIndex = Array.IndexOf(cards_info[i], j);
                        return cardPos;
                    }

            if (card_id > 10000)
            {
                if (card_id == 10001) cardPos.cardLevel = CardLevelType.levelOneCards;
                if (card_id == 10002) cardPos.cardLevel = CardLevelType.levelTwoCards;
                if (card_id == 10003) cardPos.cardLevel = CardLevelType.levelThreeCards;
                cardPos.cardIndex = card_id;
                return cardPos;
            }

            return null;
        }

        public static void showNEW_CARD(Msgs msg)
        {
            string cardLevel = CardLevelType.nobles;
            if (msg.card_id <= 90) cardLevel = CardLevelType.levelThreeCards;
            if (msg.card_id <= 70) cardLevel = CardLevelType.levelTwoCards;
            if (msg.card_id <= 40) cardLevel = CardLevelType.levelOneCards;
            GameRoom.cards_info[cardLevel][Array.IndexOf(GameRoom.cards_info[cardLevel], 0)] = msg.card_id;
        }
    }
}
