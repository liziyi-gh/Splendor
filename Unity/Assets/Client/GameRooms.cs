using System;
using Players;
using MsgStruct;
using Gems;
using JsonClasses;
using System.Collections.Generic;
using CardLevelTypes;
using System.Linq;
using Logger;
using MsgTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GameRooms
{

    public class CardPosition
    {
        public string cardLevel;
        public int cardIndex;

        public static int MISSING = -1;
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

            cardPos.cardLevel = Tools.CardNumberConvertToLevel(card_id);

            if (card_id > 10000) cardPos.cardIndex = card_id;
            else cardPos.cardIndex = CardPosition.MISSING;

            return cardPos;
        }

        public static void ShowNEW_CARD(Msgs msg)
        {
            string cardLevel = Tools.CardNumberConvertToLevel(msg.card_id);
            GameRoom.cards_info[cardLevel][Array.IndexOf(GameRoom.cards_info[cardLevel], 0)] = msg.card_id;
        }

        public static void UpdatePLAYER_OPERATION(out Msgs body_msg, Msgs head_msg, string body_str)
        {
            JObject tmp = JObject.Parse(body_str);
            if ((string)tmp["operation_type"] == Operation.DISCARD_GEMS) body_msg = Tools.MsgPLAYER_OPERATION_DISCARD_GEMS(body_str);
            else body_msg = Tools.MsgPLAYER_OPERATION(body_str);

            CardPosition cardPos = GetCardPosition(body_msg.card_id);
            int player_pos = Array.IndexOf(players_sequence, body_msg.player_id);

            switch (body_msg.operation_type)
            {
                case Operation.GET_GEMS:
                    foreach (var i in typeof(GEM).GetFields())
                    {
                        gems_last_num[i.Name.ToLower()] -= body_msg.gems[i.Name.ToLower()];
                        players[player_pos].gems[i.Name.ToLower()] += body_msg.gems[i.Name.ToLower()];
                    }
                    break;

                case Operation.BUY_CARD:
                    foreach (var i in typeof(GEM).GetFields())
                    {
                        gems_last_num[i.Name.ToLower()] += body_msg.gems[i.Name.ToLower()];
                        players[player_pos].gems[i.Name.ToLower()] -= body_msg.gems[i.Name.ToLower()];
                    }

                    if (cardPos.cardIndex == CardPosition.MISSING)
                    {
                        int foldCardPos = Array.IndexOf(players[player_pos].foldCards.ToArray(), body_msg.card_id);
                        if (foldCardPos != CardPosition.MISSING)
                        {
                            players[player_pos].foldCards.Remove(body_msg.card_id);
                            players[player_pos].foldCards_num--;
                        }
                        else
                        {
                            int cardCode = Tools.CardLevelConvertToUnknownCardcode(cardPos.cardLevel);

                            players[player_pos].foldCards.RemoveAt(Array.IndexOf(players[player_pos].foldCards.ToArray(), cardCode));
                            players[player_pos].foldCards_num--;
                        }
                    }
                    else
                    {
                        cards_info[cardPos.cardLevel][cardPos.cardIndex] = 0;
                        cards_last_num[cardPos.cardLevel] --;
                    }

                    players[player_pos].cards.Add(body_msg.card_id);
                    players[player_pos].cards_type[Tools.ReadCardType(body_msg.card_id)] ++;
                    players[player_pos].point += Tools.ReadCardPoint(body_msg.card_id);
                    break;

                case Operation.FOLD_CARD:
                    cards_info[cardPos.cardLevel][cardPos.cardIndex] = 0;
                    cards_last_num[cardPos.cardLevel] --;
                    gems_last_num[GEM.GOLDEN] -= body_msg.gems[GEM.GOLDEN];

                    players[player_pos].gems[GEM.GOLDEN] += body_msg.gems[GEM.GOLDEN];
                    players[player_pos].foldCards_num++;
                    players[player_pos].foldCards.Add(body_msg.card_id);
                    break;

                case Operation.FOLD_CARD_UNKNOWN:
                    cards_last_num[cardPos.cardLevel] --;
                    gems_last_num[GEM.GOLDEN] -= body_msg.gems[GEM.GOLDEN];

                    players[player_pos].gems[GEM.GOLDEN] += body_msg.gems[GEM.GOLDEN];
                    players[player_pos].foldCards_num++;
                    players[player_pos].foldCards.Add(body_msg.card_id);
                    break;

                case Operation.DISCARD_GEMS:
                    foreach (var i in typeof(GEM).GetFields())
                    {
                        gems_last_num[i.Name.ToLower()] += body_msg.gems[i.Name.ToLower()];
                        players[player_pos].gems[i.Name.ToLower()] -= body_msg.gems[i.Name.ToLower()];
                    }
                    break;

                default:
                    break;
            }
        }

        public static void UpdatePLAYER_GET_NOBLE(Msgs bosy_msg)
        {
            if (body_msg.nobles_id.Count() != 1) return;

            cards_info[CardLevelType.nobles][Array.IndexOf(cards_info[CardLevelType.nobles], body_msg.nobles_id[0])] = 0;

            players[Array.IndexOf(players_sequence, body_msg.player_id)].point += 3;
            players[Array.IndexOf(players_sequence, body_msg.player_id)].nobles.Add(body_msg.nobles_id[0]);
        }
    }
}
