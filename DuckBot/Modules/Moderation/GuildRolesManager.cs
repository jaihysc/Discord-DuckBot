using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckBot.Modules.Moderation
{
    public static class GuildRolesManager
    {
        /// <summary>
        /// Gets list of custom guild roles for given guild
        /// </summary>
        /// <param name="guildID">Id of target guild</param>
        /// <returns>List of GuildRoleEntry containing guild id, role name, guild role id</returns>
        public static List<GuildRoleEntry> GetGuildRoles(ulong guildID)
        {
            //Get roles from file
            var roleStorage = XmlManager.FromXmlFile<GuildRoleStorage>(CoreMethod.GetFileLocation(@"\GuildRoles.xml"));

            List<GuildRoleEntry> guildRoleEntries = new List<GuildRoleEntry>();

            //Filter roles to ones with guild ID
            foreach (var role in roleStorage.GuildRoles)
            {
                if (role.GuildID == guildID)
                {
                    guildRoleEntries.Add(role);
                }
            }

            return guildRoleEntries;
        }

        /// <summary>
        /// Adds a guild role for the specified guild
        /// </summary>
        /// <param name="guildID">Target guild id to add role</param>
        /// <param name="RoleName">Name of role to add</param>
        /// <param name="GuildRoleID">Id of guild role</param>
        public static void AddGuildRole(ulong guildID, string RoleName, ulong GuildRoleID)
        {
            //Get roles from file
            var roleStorage = XmlManager.FromXmlFile<GuildRoleStorage>(CoreMethod.GetFileLocation(@"\GuildRoles.xml"));

            //Check for overlapping role ids or names
            bool conflictingEntryExists = false;
            foreach (var roleEntry in roleStorage.GuildRoles)
            {
                if (roleEntry.GuildID == guildID)
                {
                    if (roleEntry.RoleName == RoleName) conflictingEntryExists = true;
                    if (roleEntry.GuildRoleID == GuildRoleID) conflictingEntryExists = true;
                }
            }

            //Add new entry if it does not exist
            if (conflictingEntryExists == false)
            {
                roleStorage.GuildRoles.Add(new GuildRoleEntry { GuildID = guildID, RoleName = RoleName, GuildRoleID = GuildRoleID });
            }        

            //Write back to file
            XmlManager.ToXmlFile(roleStorage, CoreMethod.GetFileLocation(@"\GuildRoles.xml"));
        }

        /// <summary>
        /// Removes a guild role for the specified guild
        /// </summary>
        /// <param name="guildID">Target guild id to remove role</param>
        /// <param name="RoleName">Name of role to removes</param>
        /// <param name="GuildRoleID">Id of guild role</param>
        public static void RemoveGuildRole(ulong guildID, string RoleName, ulong GuildRoleID)
        {
            //Get roles from file
            var roleStorage = XmlManager.FromXmlFile<GuildRoleStorage>(CoreMethod.GetFileLocation(@"\GuildRoles.xml"));

            List<GuildRoleEntry> returnRoleEntries = new List<GuildRoleEntry>();

            //Filter role entry to those not matching one to remove
            foreach (var role in roleStorage.GuildRoles)
            {
                if (role.GuildID == guildID && role.RoleName == RoleName && role.GuildRoleID == GuildRoleID)
                {
                }
                else
                {
                    returnRoleEntries.Add(role);
                }
            }

            //Write back to file

            var returnRoleStorage = new GuildRoleStorage
            {
                GuildRoles = returnRoleEntries
            };
            XmlManager.ToXmlFile(returnRoleStorage, CoreMethod.GetFileLocation(@"\GuildRoles.xml"));
        }
    }

    public class GuildRoleStorage
    {
        public List<GuildRoleEntry> GuildRoles { get; set; }
    }
    public class GuildRoleEntry
    {
        public ulong GuildID { get; set; }
        public string RoleName { get; set; }
        public ulong GuildRoleID { get; set; }
    }
}
