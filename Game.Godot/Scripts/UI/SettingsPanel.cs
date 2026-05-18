using Godot;
using System;
using Game.Godot.Adapters;

namespace Game.Godot.Scripts.UI;

public partial class SettingsPanel : Control
{
    private HSlider _volume = default!;
    private OptionButton _graphics = default!;
    private OptionButton _language = default!;
    private Button _save = default!;
    private Button _load = default!;
    private Button _close = default!;

    private const string UserId = "default";
    private const string ConfigPath = "user://settings.cfg";
    private const string ConfigSection = "settings";

    public override void _Ready()
    {
        _volume = GetNode<HSlider>("VBox/VolRow/VolSlider");
        _graphics = GetNode<OptionButton>("VBox/GraphicsRow/GraphicsOpt");
        _language = GetNode<OptionButton>("VBox/LangRow/LangOpt");
        _save = GetNode<Button>("VBox/Buttons/SaveBtn");
        _load = GetNode<Button>("VBox/Buttons/LoadBtn");
        _close = GetNode<Button>("VBox/Buttons/CloseBtn");

        _save.Pressed += OnSave;
        _load.Pressed += OnLoad;
        _close.Pressed += () => Visible = false;

        if (_graphics.ItemCount == 0)
        {
            AddOption(_graphics, "低", "low");
            AddOption(_graphics, "中", "medium");
            AddOption(_graphics, "高", "high");
            _graphics.Selected = 1;
        }
        if (_language.ItemCount == 0)
        {
            AddOption(_language, "英语", "en");
            AddOption(_language, "中文", "zh");
            AddOption(_language, "日语", "ja");
            _language.Selected = 0;
        }

        // Realtime apply handlers
        _volume.ValueChanged += OnVolumeChanged;
        _graphics.ItemSelected += OnGraphicsChanged;
        _language.ItemSelected += OnLanguageChanged;

        Visible = false;
    }

    private SqliteDataStore? Db() => GetNodeOrNull<SqliteDataStore>("/root/SqlDb");

    private static void AddOption(OptionButton optionButton, string displayText, string code)
    {
        optionButton.AddItem(displayText);
        optionButton.SetItemMetadata(optionButton.ItemCount - 1, code);
    }

    private static string GetItemCode(OptionButton optionButton, int index)
    {
        if (index < 0 || index >= optionButton.ItemCount)
        {
            return string.Empty;
        }

        var metadata = optionButton.GetItemMetadata(index);
        return metadata.VariantType == Variant.Type.Nil
            ? optionButton.GetItemText(index)
            : metadata.AsString();
    }

    private static string GetSelectedCode(OptionButton optionButton)
    {
        if (optionButton.Selected < 0)
        {
            return optionButton.ItemCount > 0 ? GetItemCode(optionButton, 0) : string.Empty;
        }

        return GetItemCode(optionButton, optionButton.Selected);
    }

    private static void SelectByCode(OptionButton optionButton, string code)
    {
        for (var i = 0; i < optionButton.ItemCount; i++)
        {
            if (string.Equals(GetItemCode(optionButton, i), code, StringComparison.OrdinalIgnoreCase))
            {
                optionButton.Selected = i;
                return;
            }
        }
    }

    private void SaveToConfig(float vol, string gfx, string lang)
    {
        var cfg = new ConfigFile();
        // Load existing to preserve unrelated keys
        cfg.Load(ConfigPath);
        cfg.SetValue(ConfigSection, nameof(vol), vol);
        cfg.SetValue(ConfigSection, nameof(gfx), gfx ?? "medium");
        cfg.SetValue(ConfigSection, nameof(lang), lang ?? "en");
        var err = cfg.Save(ConfigPath);
        if (err != Error.Ok)
        {
            GD.PushWarning($"设置面板：保存 ConfigFile 失败：{err}");
        }
    }

    private bool TryLoadFromConfig(out float vol, out string gfx, out string lang)
    {
        vol = 0.5f; gfx = "medium"; lang = "en";
        var cfg = new ConfigFile();
        var err = cfg.Load(ConfigPath);
        if (err != Error.Ok)
        {
            return false;
        }
        try
        {
            Variant v = cfg.GetValue(ConfigSection, nameof(vol), 0.5f);
            Variant g = cfg.GetValue(ConfigSection, nameof(gfx), "medium");
            Variant l = cfg.GetValue(ConfigSection, nameof(lang), "en");
            vol = v.VariantType == Variant.Type.Nil ? 0.5f : (float)v.AsDouble();
            gfx = g.VariantType == Variant.Type.Nil ? "medium" : g.AsString();
            lang = l.VariantType == Variant.Type.Nil ? "en" : l.AsString();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void MigrateFromDbIfConfigMissing()
    {
        // If config already exists, do nothing
        var cfgProbe = new ConfigFile();
        if (cfgProbe.Load(ConfigPath) == Error.Ok)
            return;

        // Attempt read from DB once and save to config
        var db = Db();
        if (db == null)
            return;
        var rows = db.Query("SELECT audio_volume, graphics_quality, language FROM settings WHERE user_id=@0;", UserId);
        if (rows.Count == 0) return;
        var r = rows[0];
        float vol = 0.5f; string gfx = "medium"; string lang = "en";
        if (r.TryGetValue("audio_volume", out var v) && v != null)
            vol = Convert.ToSingle(v);
        if (r.TryGetValue("graphics_quality", out var g) && g != null)
            gfx = g.ToString() ?? "medium";
        if (r.TryGetValue("language", out var l) && l != null)
            lang = l.ToString() ?? "en";
        SaveToConfig(vol, gfx, lang);
    }

    private void OnSave()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var vol = Mathf.Clamp((float)_volume.Value, 0, 1);
        var gfx = GetSelectedCode(_graphics);
        var lang = GetSelectedCode(_language);
        // SSoT to ConfigFile
        SaveToConfig(vol, gfx, lang);

        // Apply immediately
        ApplyVolume(vol);
        ApplyLanguage(lang);
    }

    private void OnLoad()
    {
        // Prefer ConfigFile; migrate once from DB if missing
        float vol; string gfx; string lang;
        if (!TryLoadFromConfig(out vol, out gfx, out lang))
        {
            MigrateFromDbIfConfigMissing();
            if (!TryLoadFromConfig(out vol, out gfx, out lang))
                return;
        }
        _volume.Value = vol;
        ApplyVolume(vol);
        // graphics selection
        if (!string.IsNullOrEmpty(gfx))
        {
            SelectByCode(_graphics, gfx);
        }
        // language
        if (!string.IsNullOrEmpty(lang))
        {
            SelectByCode(_language, lang);
            ApplyLanguage(GetSelectedCode(_language));
        }
    }

    public void ShowPanel() => Visible = true;

    private void OnVolumeChanged(double value)
    {
        ApplyVolume((float)value);
    }

    private void OnGraphicsChanged(long index)
    {
        var gfx = GetItemCode(_graphics, (int)index);
        ApplyGraphicsQuality(gfx);
    }

    private void OnLanguageChanged(long index)
    {
        var lang = GetItemCode(_language, (int)index);
        ApplyLanguage(lang);
    }

    private void ApplyVolume(float vol)
    {
        int bus = AudioServer.GetBusIndex("Master");
        if (bus >= 0)
        {
            AudioServer.SetBusVolumeDb(bus, Mathf.LinearToDb(Mathf.Clamp(vol, 0, 1)));
        }
    }

    private void ApplyLanguage(string lang)
    {
        if (!string.IsNullOrEmpty(lang))
            TranslationServer.SetLocale(lang);
    }

    private void ApplyGraphicsQuality(string quality)
    {
        // Map: low -> no vsync, no MSAA; medium -> vsync on, 2x; high -> vsync on, 4x/8x
        var q = (quality ?? "medium").ToLowerInvariant();
        try
        {
            if (q == "low")
                DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
            else
                DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
        }
        catch { /* not critical */ }

        var vp = GetViewport();
        if (vp != null)
        {
            int msaa = 0; // disabled
            if (q == "medium") msaa = 1; // 2x
            else if (q == "high") msaa = 2; // 4x (use 8x if needed: 3)
            // Set via dynamic property names to avoid API differences
            try { vp.Set("msaa_2d", msaa); } catch { }
            try { vp.Set("msaa_3d", msaa); } catch { }
        }
    }
}
