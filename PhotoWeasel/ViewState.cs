using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace PhotoWeasel {
	abstract class ViewState {
	}

	class NormalViewState : ViewState {
	}

	class ZoomingViewState : ViewState {
		public Boolean ZoomIn;
		public ViewState NextState;
		public int ZoomPhoto;
		public DateTime StartTime;
		public TimeSpan Runtime;
		/*public Matrix InitMatrix, FinalMatrix;

		public Matrix GetLerpMatrix(DateTime time) {
			float lerp_factor = (float)((time - StartTime).TotalSeconds / Runtime.TotalSeconds);
			return Matrix.Lerp(InitMatrix, FinalMatrix, lerp_factor);
		}*/

		public float GetLerpFactor(DateTime time) {
			return (float)((time - StartTime).TotalSeconds / Runtime.TotalSeconds);
		}

		public ZoomingViewState(bool zoom_in, int photo, ViewState next_state, TimeSpan time) : this(zoom_in, photo, next_state, time, DateTime.Now){			
		}

		public ZoomingViewState(bool zoom_in, int photo, ViewState next_state, TimeSpan time, DateTime start) {
			ZoomIn = zoom_in;
			ZoomPhoto = photo;
			NextState = next_state;
			StartTime = start;
			Runtime = time;
		}
	}

	class ZoomedViewState : ViewState {
		public int ZoomPhoto;

		public ZoomedViewState(int photo) {
			ZoomPhoto = photo;
		}
	}
}
