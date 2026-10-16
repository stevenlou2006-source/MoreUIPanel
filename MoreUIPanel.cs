using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace MoreUIPanel
{
	[BepInPlugin("cn_xc.MoreUIPanel", "MoreUIPanel", "0.1.0")]
	public class Plugin : BaseUnityPlugin
	{
		/// <summary>
		/// 注册一行显示文本
		/// </summary>
		public static void RegisterLine(string key, Func<string> textProvider)
		{
			if (!string.IsNullOrEmpty(key) && textProvider != null)
			{
				lock (_customLines)
				{
					_customLines[key] = textProvider;
				}
			}
		}

		/// <summary>
		/// 注销一行
		/// </summary>
		public static void UnregisterLine(string key)
		{
			if (!string.IsNullOrEmpty(key))
			{
				lock (_customLines)
				{
					_customLines.Remove(key);
				}
			}
		}

		/// <summary>
		/// 强制显示/隐藏
		/// </summary>
		public static bool Visible { get; set; } = true;

		// 内部存储
		private static readonly Dictionary<string, Func<string>> _customLines = new Dictionary<string, Func<string>>();

		// 配置项
		private ConfigEntry<bool> _enabled;
		private ConfigEntry<int> _fontSize;
		private ConfigEntry<int> _offsetX;
		private ConfigEntry<int> _offsetY;
		private ConfigEntry<int> _opacityPercent;
		private ConfigEntry<float> _refreshInterval;

		// 运行时
		private float _refreshTimer;
		private string _displayText = string.Empty;
		private readonly StringBuilder _builder = new StringBuilder(256);
		private GUIStyle _style;

		void Awake()
		{
			_enabled = Config.Bind("General", "Enabled", true, "启用面板 / Enable the panel");
			_fontSize = Config.Bind("General", "FontSize", 20,
				new ConfigDescription("字体大小 (10-28) / Font size", new AcceptableValueRange<int>(10, 28)));
			_offsetX = Config.Bind("General", "OffsetX", 0,
				new ConfigDescription("水平偏移 (-600~600) / Horizontal offset", new AcceptableValueRange<int>(-600, 600)));
			_offsetY = Config.Bind("General", "OffsetY", 0,
				new ConfigDescription("垂直偏移 (-600~600) / Vertical offset", new AcceptableValueRange<int>(-600, 600)));
			_opacityPercent = Config.Bind("General", "OpacityPercent", 70,
				new ConfigDescription("文字透明度百分比 (1-100) / Text opacity percentage", new AcceptableValueRange<int>(1, 100)));
			_refreshInterval = Config.Bind("General", "RefreshIntervalSeconds", 1.5f,
				new ConfigDescription("刷新间隔 (0.1~10秒) / Refresh interval", new AcceptableValueRange<float>(0.1f, 10f)));

			Logger.LogInfo("MoreUIPanel loaded — use Plugin.RegisterLine(key, provider) to add custom lines.");
		}

		void Update()
		{
			if (!_enabled.Value) return;
			_refreshTimer += Time.deltaTime;
			if (_refreshTimer >= Mathf.Max(0.1f, _refreshInterval.Value))
			{
				_refreshTimer = 0f;
				RefreshDisplayText();
			}
		}

		void RefreshDisplayText()
		{
			_builder.Clear();
			lock (_customLines)
			{
				foreach (var kvp in _customLines)
				{
					try
					{
						string text = kvp.Value?.Invoke();
						if (!string.IsNullOrWhiteSpace(text))
						{
							if (_builder.Length > 0) _builder.Append('\n');
							_builder.Append(text);
						}
					}
					catch { }
				}
			}
			_displayText = _builder.ToString();
		}

		void OnGUI()
		{
			if (!_enabled.Value || !Visible || string.IsNullOrEmpty(_displayText))
				return;

			if (_style == null)
			{
				_style = new GUIStyle(GUI.skin.label)
				{
					fontStyle = FontStyle.Bold,
					alignment = TextAnchor.LowerLeft,  // 左下角对齐
					wordWrap = false,
					richText = true
				};
			}

			_style.fontSize = _fontSize.Value;
			float opacity = Mathf.Clamp(_opacityPercent.Value, 1, 100) / 100f;
			_style.normal.textColor = new Color(1f, 1f, 1f, opacity);

			// 左下角定位：x = 10 + 偏移，y = 屏幕高度 - 10 + 偏移（但需要减去文本高度保持底部对齐）
			int x = 10 + _offsetX.Value;
			int y = Screen.height - 10 + _offsetY.Value;
			float height = _style.CalcHeight(new GUIContent(_displayText), 400f);
			float width = 400f;
			Rect rect = new Rect(x, y - height, width, height);
			GUI.Label(rect, _displayText, _style);
		}
	}
}