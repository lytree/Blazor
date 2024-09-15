﻿

using System.ComponentModel;

namespace Blazor.Hybrid.Core;

/// <summary>
/// https://learn.microsoft.com/zh-cn/dotnet/maui/platform-integration/device/sensors?view=net-maui-8.0&WT.mc_id=DT-MVP-5005078&tabs=windows
/// </summary>
public enum EnumNativeFeatures
{
    None,
    [Description("电池")]
    Battery,

    [Description("显示")]
    DeviceDisplay,

    DeviceInformation,
    Flashlight,
    SensorSpeed,
    Accelerometer,
    Barometer,
    Compass,
    Shake,
    Gyroscope,
    Magnetometer,
    Orientation,
    Geocoding,
    Geolocation,
    HapticFeedback,
    Vibration,
    Screenshot,
    TextToSpeech,
    ShareText,
    ShareUri,
    FilePicker,
    Preferences,
    IecureStorage,
}
