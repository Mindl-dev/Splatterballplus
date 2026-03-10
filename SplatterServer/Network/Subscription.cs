using SplatterServer.Properties;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Xml.Linq;

namespace SplatterServer
{
    public static class Subscription
    {
        public enum ErrorType
        {
            None,
            InvalidPassword,
            InvalidAccount,
            ServerLocked,
            UnknownError,
            InvalidVersion,
            AccessError,
            AccountDoesNotExist,
            ServerFull,
            NoMagestormAccess,
            BannedComputer,
            LoggedIn,
        }

        public static readonly String SubscriptionPage;
        public static readonly Byte[] GameVersion;

        static Subscription()
        {
            SubscriptionPage = String.Format("https://{0}/subscription.php", Settings.Default.SubscriptionHost);
            GameVersion = new[] { Convert.ToByte(49), Convert.ToByte(0), Convert.ToByte(6), Convert.ToByte(1) };
        }

	    private class AccountData
        {
            public readonly Int32 AccountId;
            public readonly ErrorType Error;
            public readonly AdminLevel Admin;
            public readonly String Username;
            public readonly Boolean MagestormPlus;

            public AccountData(Player player, String ipAddress, String serial, String username, String password)
            {
                AccountId = 0;
                Username = "";
                Admin = AdminLevel.None;
                Error = ErrorType.UnknownError;

                try
                {
                    DataTable query = MySQL.Accounts.GetAccountData(username);

                    if (query == null || query.Rows.Count == 0)
                    {
                        Error = ErrorType.AccountDoesNotExist;
                        return; // Exit early if no account exists
                    }

                    DataRow accountdata = query.Rows[0];

                    Program.ServerForm.MainLog.WriteMessage(String.Format("db passwd: {0}, sent passwd: {1}, db username: {2}, sent username: {3}", accountdata["password"], password, accountdata["username"], username), Color.DarkOrange);

                    if (accountdata["password"].ToString() == password)
                    {
                        if ((int)accountdata["AccountID"] > 0)
                        {
                            AccountId = (int)accountdata["AccountID"];
                            Username = accountdata["username"].ToString();
                            Admin = (AdminLevel)accountdata["Admin"];
                            Error = ErrorType.None;
                        }
                        else
                        {
                            Error = ErrorType.InvalidAccount;
                        }
                    }
                    else
                    {
                        Error = ErrorType.InvalidPassword;
                    }

                    if (PlayerManager.Players.GetFreePlayerCount() > 100 && (!MagestormPlus && Admin == AdminLevel.None))
                    {
                        Error = ErrorType.ServerFull;
                    }

                    if (Error == ErrorType.None)
                    {
                        Player connectedPlayer = PlayerManager.Players.FindByAccountId(AccountId);

                        if (connectedPlayer != null)
                        {
                            Error = player != connectedPlayer ? ErrorType.LoggedIn : ErrorType.None;

                            if (Error == ErrorType.LoggedIn)
                            {
                                connectedPlayer.DisconnectReason = Resources.Strings_Disconnect.MultipleLogin;
                                connectedPlayer.Disconnect = true;
                            }
                        }

                        if (serial != "Not_Found" && serial != "VMWare" && serial != "VirtualPC")
                        {
                            connectedPlayer = PlayerManager.Players.FindBySerial(serial);

                            if (connectedPlayer != null)
                            {
                                if (!connectedPlayer.IsAdmin && Admin == AdminLevel.None)
                                {
                                    Error = ErrorType.LoggedIn;
                                }
                            }
                        }

                        if (MySQL.BannedSerials.IsBanned(serial))
                        {
                            Error = ErrorType.BannedComputer;
                        }

                        if (Settings.Default.Locked && Admin == AdminLevel.None)
                        {
                            Error = ErrorType.ServerLocked;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program.ServerForm.MainLog.WriteMessage("Login Error: " + ex.Message, Color.Red);
                    Error = ErrorType.AccessError;
                } 
            }
        }

        public static void Authenticate(Player player, String username, String password, String serial, Byte[] version)
        {
            AccountData accountData = new AccountData(player, player.IpAddress, serial, username, password);

            if (accountData.Error != ErrorType.None)
            {
                Program.ServerForm.MainLog.WriteMessage(String.Format("(PID: {0}, IP: {1}, S/N: {2}) Login Error: {3}, Username: {4}", player.PlayerId, player.IpAddress, serial, accountData.Error, username), Color.DarkOrange);

                Network.Send(player, GamePacket.Outgoing.Login.Error(accountData.Error));

	            player.DisconnectReason = Resources.Strings_Disconnect.AuthenticationError;
                player.Disconnect = true;
                return;
            }

            player.AccountId = accountData.AccountId;
            player.Serial = serial;
            player.Username = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(accountData.Username);
            player.Admin = accountData.Admin;
            player.Flags |= accountData.MagestormPlus ? PlayerFlag.MagestormPlus : PlayerFlag.None;

            if (BitConverter.ToInt32(version, 0) != BitConverter.ToInt32(GameVersion, 0))
            {
                if (player.IsAdmin)
                {
                    Program.ServerForm.MainLog.WriteMessage(String.Format("(PID: {0}, AID: {1}) {2} has a version mismatch. Allowing anyway. ({3})", player.PlayerId, player.AccountId, player.Username, player.Admin), Color.DarkOrange);
                }
                else
                {
                    Network.Send(player, GamePacket.Outgoing.Login.Error(ErrorType.InvalidVersion));

	                player.DisconnectReason = Resources.Strings_Disconnect.InvalidVersion;
                    player.Disconnect = true;
                    return;
                }
            }

            Network.Send(player, GamePacket.Outgoing.Login.Connected(player));
            Network.Send(player, GamePacket.Outgoing.Player.SendPlayerId(player));

			MySQL.OnlineAccounts.SetOnline(player.AccountId);

            Program.ServerForm.MainLog.WriteMessage(String.Format("(PID: {0}, AID: {1}, S/N: {2}) {3} has connected.", player.PlayerId, player.AccountId, serial, player.Username), Color.MediumSlateBlue);
        }
    }
}