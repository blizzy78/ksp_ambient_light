/*
Ambient Light Adjustment - Modify ambient lighting in Kerbal Space Program.
Copyright (C) 2014 Maik Schreiber

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;

namespace AmbientLightAdjustment {
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	internal class AmbientLightAdjustment : MonoBehaviour {
		internal static int VERSION = 1;

		private static readonly string SETTINGS_FILE = KSPUtil.ApplicationRootPath + "GameData/blizzy/AmbientLightAdjustment/settings.dat";
		private const int AUTO_HIDE_DELAY = 5;

		private IButton button;
		private AmbienceSetting setting;
		private AmbienceSetting secondSetting;
		private Color defaultAmbience;
		private bool listenToSliderChange = true;

		public void Start() {
			if (isRelevantScene()) {
				button = ToolbarManager.Instance.add("AmbientLightAdjustment", "adjustLevels");
				button.TexturePath = "blizzy/AmbientLightAdjustment/contrast";
				button.ToolTip = "Ambient Light Adjustment";
				button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT, GameScenes.TRACKSTATION);
				button.OnClick += (e) => {
					switch (e.MouseButton) {
						case 1:
							resetToDefaultAmbience();
							break;
						case 2:
							switchToSecondSetting();
							break;
						default:
							toggleAdjustmentUI();
							break;
					}
				};

				loadSettings();
			}
		}

		public void Destroy() {
			if (button != null) {
				button.Destroy();
				button = null;
			}
		}

		private void loadSettings() {
			ConfigNode settings = ConfigNode.Load(SETTINGS_FILE) ?? new ConfigNode();
			if (settings.HasNode("ambience")) {
				ConfigNode ambienceNode = settings.GetNode("ambience");
				ConfigNode[] settingNodes = ambienceNode.GetNodes("setting");
				if (settingNodes.Length >= 1) {
					setting = AmbienceSetting.create(settingNodes[0]);
				}
				if (settingNodes.Length >= 2) {
					secondSetting = AmbienceSetting.create(settingNodes[1]);
				}
			}
		}

		private void saveSettings() {
			ConfigNode root = new ConfigNode();
			ConfigNode ambienceNode = root.AddNode("ambience");
			setting.save(ambienceNode.AddNode("setting"));
			secondSetting.save(ambienceNode.AddNode("setting"));
			root.Save(SETTINGS_FILE);
		}

		private bool isRelevantScene() {
			return HighLogic.LoadedSceneIsFlight || (HighLogic.LoadedScene == GameScenes.TRACKSTATION);
		}

		private void toggleAdjustmentUI() {
			if (button.Drawable == null) {
				showAdjustmentUI();
			} else {
				hideAdjustmentUI();
			}
		}

		private void showAdjustmentUI() {
			AdjustmentDrawable adjustment = new AdjustmentDrawable();

			adjustment.OnLevelChanged += () => {
				if (listenToSliderChange) {
					setting.Level = adjustment.Level;
					setting.UseDefaultAmbience = false;
					saveSettings();
					startAutoHide();
				}
			};

			button.Drawable = adjustment;
			updateSliderFromSetting();

			startAutoHide();
		}

		private void hideAdjustmentUI() {
			stopAutoHide();
			button.Drawable = null;
		}

		private void startAutoHide() {
			stopAutoHide();
			StartCoroutine("doAutoHide");
		}

		private void stopAutoHide() {
			StopCoroutine("doAutoHide");
		}

		private IEnumerator doAutoHide() {
			yield return new WaitForSeconds(AUTO_HIDE_DELAY);
			hideAdjustmentUI();
		}

		private void resetToDefaultAmbience() {
			setting.Level = defaultAmbience.grayscale;
			setting.UseDefaultAmbience = true;
			updateSliderFromSetting();
			saveSettings();
		}

		private void switchToSecondSetting() {
			AmbienceSetting temp = setting;
			setting = secondSetting;
			secondSetting = temp;
			updateSliderFromSetting();
			saveSettings();
		}

		private void updateSliderFromSetting() {
			if (button.Drawable != null) {
				listenToSliderChange = false;
				((AdjustmentDrawable) button.Drawable).Level = setting.Level;
				listenToSliderChange = true;
			}
		}

		public void LateUpdate() {
			if (isRelevantScene()) {
				defaultAmbience = RenderSettings.ambientLight;

				if (setting == null) {
					setting = new AmbienceSetting() {
						UseDefaultAmbience = true,
						Level = defaultAmbience.grayscale
					};
				}
				if (secondSetting == null) {
					secondSetting = new AmbienceSetting() {
						UseDefaultAmbience = true,
						Level = defaultAmbience.grayscale
					};
				}

				if (!setting.UseDefaultAmbience) {
					Color ambience = defaultAmbience;
					ambience.r = setting.Level;
					ambience.g = setting.Level;
					ambience.b = setting.Level;
					RenderSettings.ambientLight = ambience;
				}
			}
		}
	}
}
