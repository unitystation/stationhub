using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace GameMode.DBus
{
    [DBusInterface("com.feralinteractive.GameMode")]
    interface IGameMode : IDBusObject
    {
        Task<int> RegisterGameAsync(int arg0);
        Task<int> UnregisterGameAsync(int arg0);
        Task<int> QueryStatusAsync(int arg0);
        Task<int> RegisterGameByPIDAsync(int arg0, int arg1);
        Task<int> UnregisterGameByPIDAsync(int arg0, int arg1);
        Task<int> QueryStatusByPIDAsync(int arg0, int arg1);
        Task<int> RegisterGameByPIDFdAsync(CloseSafeHandle arg0, CloseSafeHandle arg1);
        Task<int> UnregisterGameByPIDFdAsync(CloseSafeHandle arg0, CloseSafeHandle arg1);
        Task<int> QueryStatusByPIDFdAsync(CloseSafeHandle arg0, CloseSafeHandle arg1);
        Task<int> RefreshConfigAsync();
        Task<(int, ObjectPath)[]> ListGamesAsync();
        Task<IDisposable> WatchGameRegisteredAsync(Action<(int, ObjectPath)> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchGameUnregisteredAsync(Action<(int, ObjectPath)> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<GameModeProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class GameModeProperties
    {
        private int _ClientCount = default(int);
        public int ClientCount
        {
            get
            {
                return _ClientCount;
            }

            set
            {
                _ClientCount = (value);
            }
        }
    }

    static class GameModeExtensions
    {
        public static Task<int> GetClientCountAsync(this IGameMode o) => o.GetAsync<int>("ClientCount");
    }
}