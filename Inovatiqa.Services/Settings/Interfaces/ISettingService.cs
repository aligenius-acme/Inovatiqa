using Inovatiqa.Database.Models;
using Inovatiqa.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Inovatiqa.Services.Settings.Interfces
{
    public partial interface ISettingService
    {
        void DeleteSetting(Setting setting);

        void DeleteSettings(IList<Setting> settings);

        Setting GetSettingById(int settingId);

        List<Setting> LoadSliderSettings(int typeId, int storeId = 0);

        T LoadSetting<T>(int storeId = 0) where T : ISettings, new();

        ISettings LoadSetting(Type type, int storeId = 0);

        T GetSettingByKey<T>(string key, T defaultValue = default,
            int storeId = 0, bool loadSharedValueIfNotFound = false);

        IList<Setting> GetAllSettings();

        void SaveSettingOverridablePerStore<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector,
            bool overrideForStore, int storeId = 0, bool clearCache = true) where T : ISettings, new();

        void SaveSetting<T>(T settings, int storeId = 0) where T : ISettings, new();

        void SaveSetting<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector,
            int storeId = 0, bool clearCache = true) where T : ISettings, new();

        string GetSettingKey<TSettings, T>(TSettings settings, Expression<Func<TSettings, T>> keySelector)
            where TSettings : ISettings, new();

        void SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true);
    }
}