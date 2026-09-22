using System;
using System.Collections.Generic;
using NUnit.Framework;
using SoulsLike.Services;
using SoulsLike.Services.Audio;
using SoulsLike.Services.Audio.Data;
using SoulsLike.Services.Save;
using SoulsLike.Services.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace SoulsLike.Editor.Tests.Settings
{
    public sealed class SettingsServiceTests
    {
        private SettingsDefaultsData _defaults;

        [SetUp]
        public void SetUp()
        {
            _defaults = ScriptableObject.CreateInstance<SettingsDefaultsData>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_defaults);
        }

        [Test]
        public void FirstLaunchCreatesAndPersistsIndependentDefaults()
        {
            var save = new FakeSaveService();
            SettingsService service = CreateService(save, new FakeInputService(), out _);

            service.Initialize();

            Assert.That(save.SaveCalls, Is.EqualTo(1));
            Assert.That(service.Current.Audio.MasterVolume, Is.EqualTo(1f));

            service.BeginEdit();
            service.Draft.Audio.MasterVolume = 0.25f;

            Assert.That(service.Current.Audio.MasterVolume, Is.EqualTo(1f));
            Assert.That(_defaults.CreateCopy().Audio.MasterVolume, Is.EqualTo(1f));
        }

        [Test]
        public void PreviewCancelAndApplyRespectTransactionBoundary()
        {
            var save = new FakeSaveService();
            SettingsService service = CreateService(save, new FakeInputService(), out FakeAudioService audio);
            service.Initialize();
            int startupSaves = save.SaveCalls;

            service.BeginEdit();
            service.Draft.Audio.MasterVolume = 0.25f;
            service.Preview(SettingsSection.Audio);

            Assert.That(audio.Applied.MasterVolume, Is.EqualTo(0.25f));
            Assert.That(service.Current.Audio.MasterVolume, Is.EqualTo(1f));
            Assert.That(save.SaveCalls, Is.EqualTo(startupSaves));

            service.CancelEdit();

            Assert.That(audio.Applied.MasterVolume, Is.EqualTo(1f));
            Assert.That(save.SaveCalls, Is.EqualTo(startupSaves));

            service.BeginEdit();
            service.Draft.Audio.MasterVolume = 0.4f;
            Assert.That(service.Apply(), Is.EqualTo(SettingsApplyResult.Applied));

            Assert.That(service.Current.Audio.MasterVolume, Is.EqualTo(0.4f));
            Assert.That(save.SaveCalls, Is.EqualTo(startupSaves + 1));

            SettingsService restarted = CreateService(save, new FakeInputService(), out _);
            restarted.Initialize();
            Assert.That(restarted.Current.Audio.MasterVolume, Is.EqualTo(0.4f));
        }

        [Test]
        public void RiskyDisplayChangeDoesNotPersistUntilDecision()
        {
            var save = new FakeSaveService();
            SettingsService service = CreateService(save, new FakeInputService(), out _, out FakeGraphicsSettingsApplier graphics);
            service.Initialize();
            int startupSaves = save.SaveCalls;

            service.BeginEdit();
            service.Draft.Graphics.DisplayMode = SettingsDataUtility.Copy(graphics.Modes[1]);

            Assert.That(service.Apply(), Is.EqualTo(SettingsApplyResult.RequiresDisplayConfirmation));
            Assert.That(save.SaveCalls, Is.EqualTo(startupSaves));
            Assert.That(graphics.LastApplied.DisplayMode.Width, Is.EqualTo(2560));

            service.RevertPendingDisplayChange();

            Assert.That(service.Current.Graphics.DisplayMode.Width, Is.EqualTo(1920));
            Assert.That(graphics.LastApplied.DisplayMode.Width, Is.EqualTo(1920));
            Assert.That(save.SaveCalls, Is.EqualTo(startupSaves + 1));
        }

        [Test]
        public void InvalidBindingOverridesRecoverWithoutBlockingStartup()
        {
            var save = new FakeSaveService
            {
                Data = new GameSettingsData
                {
                    Controls = new ControlsSettingsData { BindingOverridesJson = "bad-json" }
                }
            };
            var input = new FakeInputService { ThrowOnInvalidJson = true };
            SettingsService service = CreateService(save, input, out _);

            LogAssert.Expect(
                LogType.Error,
                "[SettingsService] Binding overrides are invalid and were reset. Invalid binding override JSON.");
            Assert.DoesNotThrow(service.Initialize);
            Assert.That(service.Current.Controls.BindingOverridesJson, Is.Empty);
            Assert.That(input.ClearCalls, Is.EqualTo(1));
        }

        [TestCase(-0.5f, 0f)]
        [TestCase(1.5f, 1f)]
        public void Preview_ClampsDynamicCameraSensitivity(float sensitivity, float expected)
        {
            SettingsService service = CreateService(new FakeSaveService(), new FakeInputService(), out _);
            service.Initialize();
            service.BeginEdit();
            service.Draft.Camera.Sensitivity = sensitivity;

            service.Preview(SettingsSection.Camera);

            Assert.That(service.Draft.Camera.Sensitivity, Is.EqualTo(expected));
        }

        [TestCase(0, 0, 1920, 1080)]
        [TestCase(2400, 1400, 2560, 1440)]
        public void Preview_AdaptsGraphicsToCurrentHardware(
            int width, int height, int expectedWidth, int expectedHeight)
        {
            SettingsService service = CreateService(new FakeSaveService(), new FakeInputService(), out _);
            service.Initialize();
            service.BeginEdit();
            service.Draft.Graphics.WindowMode = FullScreenMode.ExclusiveFullScreen;
            service.Draft.Graphics.DisplayMode.Width = width;
            service.Draft.Graphics.DisplayMode.Height = height;
            service.Draft.Graphics.QualityLevelName = "Unknown quality preset";

            service.Preview(SettingsSection.Graphics);

            Assert.That(service.Draft.Graphics.WindowMode, Is.EqualTo(FullScreenMode.FullScreenWindow));
            Assert.That(service.Draft.Graphics.DisplayMode.Width, Is.EqualTo(expectedWidth));
            Assert.That(service.Draft.Graphics.DisplayMode.Height, Is.EqualTo(expectedHeight));
            Assert.That(service.Draft.Graphics.QualityLevelName,
                Is.EqualTo(QualitySettings.names[QualitySettings.GetQualityLevel()]));
        }

        [Test]
        public void Preview_MissingRequiredCameraSettingsFailsAtUse()
        {
            SettingsService service = CreateService(new FakeSaveService(), new FakeInputService(), out _);
            service.Initialize();
            service.BeginEdit();
            service.Draft.Camera = null;

            Assert.Throws<NullReferenceException>(() => service.Preview(SettingsSection.Camera));
        }

        private SettingsService CreateService(
            FakeSaveService save,
            FakeInputService input,
            out FakeAudioService audio)
        {
            return CreateService(save, input, out audio, out _);
        }

        private SettingsService CreateService(
            FakeSaveService save,
            FakeInputService input,
            out FakeAudioService audio,
            out FakeGraphicsSettingsApplier graphics)
        {
            audio = new FakeAudioService();
            graphics = new FakeGraphicsSettingsApplier();
            return new SettingsService(save, _defaults, audio, input, graphics);
        }

        private sealed class FakeSaveService : ISaveService
        {
            public GameSettingsData Data;
            public int SaveCalls;

            public bool Exists(string fileName) => Data != null;

            public void Save<T>(string fileName, T data)
            {
                SaveCalls++;
                Data = SettingsDataUtility.Copy((GameSettingsData)(object)data);
            }

            public T Load<T>(string fileName)
            {
                return (T)(object)SettingsDataUtility.Copy(Data);
            }

            public void Delete(string fileName)
            {
                Data = null;
            }

            public void DeleteAll()
            {
                Data = null;
            }
        }

        private sealed class FakeAudioService : IAudioService
        {
            public AudioSettingsData Applied { get; private set; } = new();
            public float BaseVolume => 1f;
            public IAudioSettingsData CurrentSettings => Applied;

            public void AddObserver(SoulsLike.Services.IObserver<IAudioSettingsData> observer)
            {
            }

            public void RemoveObserver(SoulsLike.Services.IObserver<IAudioSettingsData> observer)
            {
            }

            public void UpdateSettings(IAudioSettingsData newSettings)
            {
                Applied = new AudioSettingsData
                {
                    MasterVolume = newSettings.MasterVolume,
                    MusicVolume = newSettings.MusicVolume,
                    SfxVolume = newSettings.SfxVolume,
                    MuteAll = newSettings.MuteAll
                };
            }

            public void ApplySettings(AudioSettingsData settings)
            {
                UpdateSettings(settings);
            }
        }

        private sealed class FakeInputService : IInputService
        {
            public bool ThrowOnInvalidJson;
            public int ClearCalls;
            public ProjectInputActions.CharacterActions CharacterActions => default;
            public ProjectInputActions.UIActions UIActions => default;
            public InputAction OpenInventoryAction => null;
            public InputAction OpenEquipmentAction => null;
            public InputAction ToggleLoreAction => null;
            public InputAction ToggleSimpleViewAction => null;
            public InputAction ToggleCheatsAction => null;
            public InputAction UnequipAction => null;
            public InputAction UiBackAction => null;
            public bool WasUiBackConsumedThisFrame => false;

            public void ConsumeUiBack()
            {
            }

            public string SaveBindingOverrides() => string.Empty;

            public void LoadBindingOverrides(string bindingOverridesJson)
            {
                if (ThrowOnInvalidJson && bindingOverridesJson == "bad-json")
                {
                    throw new ArgumentException("Invalid binding override JSON.");
                }
            }

            public void ClearBindingOverrides()
            {
                ClearCalls++;
            }
        }

        private sealed class FakeGraphicsSettingsApplier : IGraphicsSettingsApplier
        {
            public readonly List<DisplayModeData> Modes = new()
            {
                new DisplayModeData
                {
                    Width = 1920,
                    Height = 1080,
                    RefreshRateNumerator = 60,
                    RefreshRateDenominator = 1
                },
                new DisplayModeData
                {
                    Width = 2560,
                    Height = 1440,
                    RefreshRateNumerator = 60,
                    RefreshRateDenominator = 1
                }
            };

            public GraphicsSettingsData LastApplied { get; private set; }
            public SettingsCapabilities Capabilities => new(false, true);

            public IReadOnlyList<DisplayModeData> GetAvailableDisplayModes() => Modes;

            public GraphicsSettingsData GetCurrentSettings()
            {
                return new GraphicsSettingsData
                {
                    WindowMode = FullScreenMode.FullScreenWindow,
                    DisplayMode = SettingsDataUtility.Copy(Modes[0]),
                    QualityLevelName = QualitySettings.names[QualitySettings.GetQualityLevel()]
                };
            }

            public void Apply(GraphicsSettingsData settings)
            {
                LastApplied = SettingsDataUtility.Copy(settings);
            }
        }
    }
}
