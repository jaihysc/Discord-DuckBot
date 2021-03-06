﻿using DuckBot.Modules.Csgo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Models
{
    public class ItemListType
    {
        public Rarity Rarity { get; set; }
        public WeaponType? WeaponType { get; set; }
        public WeaponType? BlackListWeaponType { get; set; }
    }


    public class UserSkinStorageRootobject
    {
        public long SkinAmount { get; set; }
        public List<UserSkinEntry> UserSkinEntries { get; set; }
    }
    public class UserSkinEntry
    {
        public ulong OwnerID { get; set; }
        public string ClassId { get; set; }
        public DateTime UnboxDate { get; set; }
        public string MarketName { get; set; }
    }
}
