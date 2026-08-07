using System;

namespace SoulsLike.Services.Storage
{
    public interface IStorageRegistry
    {
        void SaveData<T>(string key, T data);
        void SaveData<T>(Enum key, T data);
        T GetData<T>(string key, T defaultValue = default);
        T GetData<T>(Enum key, T defaultValue = default);
        bool HasData(string key);
        bool HasData(Enum key);
        void DeleteData(string key);
        void DeleteData(Enum key);
    }
}
