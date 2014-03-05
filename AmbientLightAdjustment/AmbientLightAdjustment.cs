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
		private float defaultLevel;
		private float level;

		public void Start() {
			if (isRelevantScene()) {
				button = ToolbarManager.Instance.add("AmbientLightAdjustment", "adjustLevels");
				button.TexturePath = "blizzy/AmbientLightAdjustment/contrast";
				button.ToolTip = "Ambient Light Adjustment (Right-Click to Reset)";
				button.OnClick += (e) => {
					if (e.MouseButton == 1) {
						resetToDefault();
					} else {
						toggleAdjustmentUI();
					}
				};

				defaultLevel = RenderSettings.ambientLight.r;
				level = defaultLevel;
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
			adjustment.Level = level;

			adjustment.OnLevelChanged += () => {
				level = adjustment.Level;
			};

			button.Drawable = adjustment;
		}

		private void hideAdjustmentUI() {
			button.Drawable = null;
		}

		private void resetToDefault() {
			if (button.Drawable != null) {
				((AdjustmentDrawable) button.Drawable).Level = defaultLevel;
			} else {
				level = defaultLevel;
			}
		}

		public void LateUpdate() {
			if (isRelevantScene()) {
				Color ambience = RenderSettings.ambientLight;
				ambience.r = level;
				ambience.g = level;
				ambience.b = level;
				RenderSettings.ambientLight = ambience;
			}
		}
	}
}
