using System;
using Helper;

namespace SplatterServer
{
    public enum CTFlagState
    {
        InHomeShrine,
        OnEnemyPlayer,
        OnAnotherPlayer,
        OnGround,
    }

    public class CTFlag
    {
        private readonly Object _syncRoot = new Object();
        
        public Object SyncRoot
        {
            get { return _syncRoot; }
        }

        private readonly Team _team;
        private readonly Int16 _objectId ;
        private CTFlagState _flagState;
        private ArenaPlayer _flagPlayer;
        private Sign _flagSign;
        
        public CTFlagState FlagState
        {
            get
            {
                return _flagState;
            }
        }

        public ArenaPlayer FlagPlayer
        {
            get
            {
                return _flagPlayer;
            }
        }

        public Sign FlagSign
        {
            get
            {
                return _flagSign;
            }
        }

        public Int16 ObjectId
        {
            get
            {
                return _objectId;
            }
        }

        public CTFlag(Team team, Int16 objectId)
        {
            _flagState = CTFlagState.InHomeShrine;
            _flagPlayer = null;
            _flagSign = null;
            _team = team;
            _objectId = objectId;
        }

        public CTFlagState ChangeState(ArenaPlayer arenaPlayer)
        {
            lock (SyncRoot)
            {
                switch (FlagState)
                {
                    case CTFlagState.InHomeShrine:
                    {
                        if (arenaPlayer.ActiveTeam != _team && arenaPlayer.ActiveTeam != Team.NoTeam)
                        {
                            _flagSign = null;
                            _flagPlayer = arenaPlayer;
                            _flagState = CTFlagState.OnEnemyPlayer;
                        }

                        break;
                    }
                    case CTFlagState.OnEnemyPlayer:
                    {
                        if (arenaPlayer.ActiveTeam != _team)
                        {
                            ResetFlag();
                        }

                        break;
                    }
                    case CTFlagState.OnGround:
                    {
                        if (arenaPlayer.ActiveTeam != _team && arenaPlayer.ActiveTeam != Team.NoTeam)
                        {
                            _flagSign = null;
                            _flagPlayer = arenaPlayer;
                            _flagState = CTFlagState.OnEnemyPlayer;
                        }
                        else
                        {
                            ResetFlag();
                        }
                        break;
                    }
                }
            }
            return FlagState;
        }

        public CTFlagState ChangeState(Sign sign)
        {
            lock (SyncRoot)
            {
                switch (FlagState)
                {
                    case CTFlagState.OnEnemyPlayer:
                    {
                        _flagPlayer = null;
                        _flagSign = sign;
                        _flagState = CTFlagState.OnGround;
                        break;
                    }
                }
            }
            return FlagState;
        }

        public void ResetFlag()
        {
            lock (SyncRoot)
            {
                _flagPlayer = null;
                _flagSign = null;
                _flagState = CTFlagState.InHomeShrine;
            }
        }
    }
}
