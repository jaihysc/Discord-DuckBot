﻿using Discord.Commands;
using DuckBot.Modules.Finance;
using DuckBot.Modules.UserActions;
using DuckBot_ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DuckBot.Modules.Commands.Preconditions
{
    class UserStorageCheckerPrecondition : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider _services)
        {
            //Create xml user credit entry if user does not exist
            if (!File.Exists(CoreMethod.GetFileLocation(@"\UserStorage") + @"\" + context.Message.Author.Id + ".xml"))
            {
                //Create user profile
                UserXmlDataStorage.CreateNewUserXmlEntry(context as SocketCommandContext);
            }

            
            //Create user stock entry if stock entry does not exist
            if (!File.Exists(CoreMethod.GetFileLocation(@"\UserStocks") + @"\" + context.User.Id.ToString() + @"\UserStockPortfolio.xml"))
            {
                //Create user profile
                //Write user stock amount
                var userStockRecord = new UserStockStorage
                {
                    UserStock = new List<UserStock>
                    {
                        //new UserStock {StockTicker="DUCK", StockAmount=0, StockBuyPrice=0 }
                    }
                };

                System.IO.Directory.CreateDirectory(CoreMethod.GetFileLocation(@"\UserStocks") + @"\" + context.User.Id.ToString());
                XmlManager.ToXmlFile(userStockRecord, CoreMethod.GetFileLocation(@"\UserStocks") + @"\" + context.User.Id.ToString() + @"\UserStockPortfolio.xml");
            }
            
            return PreconditionResult.FromSuccess();
        }
    }
}