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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;

namespace AmbientLightAdjustment {
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	internal class AmbientLightAdjustment : MonoBehaviour {
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
			}
		}

		public void Destroy() {
			if (button != null) {
				button.Destroy();
				button = null;
			}
		}

		private bool isRelevantScene() {
			return HighLogic.LoadedScene == GameScenes.FLIGHT;
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
					setting.level = adjustment.Level;
					setting.useDefaultAmbience = false;
				}
			};

			button.Drawable = adjustment;
			updateSliderFromSetting();
		}

		private void hideAdjustmentUI() {
			button.Drawable = null;
		}

		private void resetToDefaultAmbience() {
			setting.level = defaultAmbience.grayscale;
			setting.useDefaultAmbience = true;
			updateSliderFromSetting();
		}

		private void switchToSecondSetting() {
			AmbienceSetting temp = setting;
			setting = secondSetting;
			secondSetting = temp;
			updateSliderFromSetting();
		}

		private void updateSliderFromSetting() {
			if (button.Drawable != null) {
				listenToSliderChange = false;
				((AdjustmentDrawable) button.Drawable).Level = setting.level;
				listenToSliderChange = true;
			}
		}

		public void LateUpdate() {
			if (isRelevantScene()) {
				defaultAmbience = RenderSettings.ambientLight;

				if (setting == null) {
					setting = new AmbienceSetting() {
						useDefaultAmbience = true,
						level = defaultAmbience.grayscale
					};
				}
				if (secondSetting == null) {
					secondSetting = new AmbienceSetting() {
						useDefaultAmbience = true,
						level = defaultAmbience.grayscale
					};
				}

				if (!setting.useDefaultAmbience) {
					Color ambience = defaultAmbience;
					ambience.r = setting.level;
					ambience.g = setting.level;
					ambience.b = setting.level;
					RenderSettings.ambientLight = ambience;
				}
			}
		}
	}
}
