using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D10;
using D3DDevice = SlimDX.Direct3D10.Device;

namespace PhotoWeasel {
	public partial class MainForm : Form {
		SwapChain m_SwapChain;
		D3DDevice m_D3DDevice;
		RenderTargetView m_RenderTargetView;
		DepthStencilView m_DepthStencilView;
		Effect m_Effect;
		SlimDX.Direct3D10.Buffer m_Vertices;
		Texture2D m_Tex;
		ViewState m_CurrentViewState;

		ScrollableControl m_Window;

		const int InputStride = 24;

		const int Border = 24;
		
		int m_PhotoSize = 300;
		float m_ScrollPos = 0;

		DateTime m_Start;

		public MainForm() {
			InitializeComponent();
			m_Window = splitContainer1.Panel2;
			m_CurrentViewState = new NormalViewState();
			InitD3D();
			SlimDX.Windows.MessagePump.Run(this, Render);
		}

		private void InitD3D() {
			{
				SwapChainDescription swap_chain_desc = new SwapChainDescription();
				swap_chain_desc.BufferCount = 2;
				swap_chain_desc.OutputHandle = m_Window.Handle;
				swap_chain_desc.IsWindowed = true;

				swap_chain_desc.ModeDescription = new ModeDescription(m_Window.Width, m_Window.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

				swap_chain_desc.SampleDescription = new SampleDescription(1, 0);
				swap_chain_desc.SwapEffect = SwapEffect.Discard;
				swap_chain_desc.Usage = Usage.RenderTargetOutput;

				if (D3DDevice.CreateWithSwapChain(null, DriverType.Hardware, 0, swap_chain_desc, out m_D3DDevice, out m_SwapChain).IsFailure) {
					throw new Exception("Bad stuff happened");
				}

				m_D3DDevice.Factory.SetWindowAssociation(m_Window.Handle, WindowAssociationFlags.IgnoreAll);
			}

			using (Texture2D back_buffer = Texture2D.FromSwapChain<Texture2D>(m_SwapChain, 0)) {
				m_RenderTargetView = new RenderTargetView(m_D3DDevice, back_buffer);
			}

			{
				var tex_desc = new Texture2DDescription();
				tex_desc.Width = m_Window.Width;
				tex_desc.Height = m_Window.Height;
				tex_desc.SampleDescription = new SampleDescription(1,0);
				tex_desc.Format = Format.D32_Float;
				tex_desc.MipLevels = 1;
				tex_desc.ArraySize = 1;
				tex_desc.BindFlags = BindFlags.DepthStencil;
				tex_desc.CpuAccessFlags = CpuAccessFlags.None;
				tex_desc.Usage = ResourceUsage.Default;
				tex_desc.OptionFlags = ResourceOptionFlags.None;

				var depth_buffer = new Texture2D(m_D3DDevice, tex_desc);

				m_DepthStencilView = new DepthStencilView(m_D3DDevice, depth_buffer);
			}

			m_D3DDevice.OutputMerger.SetTargets(m_DepthStencilView, m_RenderTargetView);
			//m_D3DDevice.OutputMerger.SetTargets(m_RenderTargetView);
			{
				Viewport viewport = new Viewport(0, 0, m_Window.Width, m_Window.Height, 0, 1);
				m_D3DDevice.Rasterizer.SetViewports(viewport);
			}

			m_Effect = Effect.FromFile(m_D3DDevice, "effect.fx", "fx_4_0");

			m_Tex = Texture2D.FromFile(m_D3DDevice, "IMG_4788.jpg");

			GenerateGeometry();

			m_Start = DateTime.Now;
		}

		private void GenerateGeometry() {
			using (var stream = new DataStream(4 * InputStride, true, true)) {
				for (int i = 0; i < 4; i++) {
					float x = (i % 2 == 0) ? -0.5f : 0.5f;
					float y = (i / 2 == 0) ? -0.5f : 0.5f;
					stream.Write(new Vector4(x, y, 0.0f, 1.0f));
					stream.Write(new Vector2(0.5f - x * 1.03f, 0.5f - y * 1.03f));
				}

				stream.Position = 0;

				m_Vertices = new SlimDX.Direct3D10.Buffer(m_D3DDevice, stream, new BufferDescription() {
					BindFlags = BindFlags.VertexBuffer,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,
					SizeInBytes = 4 * InputStride,
					Usage = ResourceUsage.Default
				});
			}
		}

		private void Render() {
			m_D3DDevice.ClearRenderTargetView(m_RenderTargetView, Color.Black);
			m_D3DDevice.ClearDepthStencilView(m_DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

			m_D3DDevice.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
			m_D3DDevice.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_Vertices, InputStride, 0));

			EffectTechnique technique = m_Effect.GetTechniqueByName("Render");
			for (int i = 0; i < technique.Description.PassCount; i++) {
				EffectPass pass = technique.GetPassByIndex(i);
				var layout = new InputLayout(m_D3DDevice, pass.Description.Signature, new[] {
					new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0),
					//new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
				});

				m_Effect.GetVariableByName("View").AsMatrix().SetMatrix(Matrix.Translation(m_Window.Width / 2.0f, m_ScrollPos + m_Window.Height / 2.0f, 0) * Matrix.LookAtLH(new Vector3(0, 0, 4), Vector3.Zero, Vector3.UnitY));
				m_Effect.GetVariableByName("Projection").AsMatrix().SetMatrix(Matrix.OrthoLH(m_Window.Width, m_Window.Height, 1, 10));

				m_D3DDevice.InputAssembler.SetInputLayout(layout);

				ShaderResourceView resource_view = new ShaderResourceView(m_D3DDevice, m_Tex);

				m_Effect.GetVariableByName("tex2D").AsResource().SetResource(resource_view);

				for (int j = 0; j < 4; j++) {
					pass.Apply();

					float imageAR = (float)m_Tex.Description.Width / m_Tex.Description.Height;
					float viewportAR = (float)m_Window.Width / m_Window.Height;

					if (m_CurrentViewState is ZoomingViewState && (m_CurrentViewState as ZoomingViewState).ZoomPhoto == j) {
						float lerp_factor = (m_CurrentViewState as ZoomingViewState).GetLerpFactor(DateTime.Now);

						bool zoom_in = (m_CurrentViewState as ZoomingViewState).ZoomIn;

						if (lerp_factor > 1) {
							m_CurrentViewState = (m_CurrentViewState as ZoomingViewState).NextState;
							lerp_factor = 1;
						}

						if (!zoom_in) {
							lerp_factor = 1 - lerp_factor;
						}
						
						//m_Effect.GetVariableByName("AspectRatio").AsScalar().Set(imageAR / (float)Math.Pow(viewportAR, lerp_factor));
						m_Effect.GetVariableByName("AspectRatio").AsScalar().Set(Lerp(imageAR, imageAR / viewportAR, lerp_factor));

						m_Effect.GetVariableByName("World").AsMatrix().SetMatrix(Matrix.Lerp(GetWorldTransform(j), GetZoomTransform(), lerp_factor));

					} else if (m_CurrentViewState is ZoomedViewState && (m_CurrentViewState as ZoomedViewState).ZoomPhoto == j) {
						m_Effect.GetVariableByName("AspectRatio").AsScalar().Set(imageAR / viewportAR);
						m_Effect.GetVariableByName("World").AsMatrix().SetMatrix(GetZoomTransform());
					} else {
						m_Effect.GetVariableByName("AspectRatio").AsScalar().Set(imageAR);
						m_Effect.GetVariableByName("World").AsMatrix().SetMatrix(GetWorldTransform(j));
					}
					m_D3DDevice.Draw(4, 0);
				}
			}

			m_SwapChain.Present(1, PresentFlags.None);
		}

		private Matrix GetLerpMatrix(float i) {
			int a = (int)Math.Floor(i);
			int b = a + 1;

			return Matrix.Lerp(GetWorldTransform(a), GetWorldTransform(b), i - a);
		}

		private Matrix GetWorldTransform2(int pos) {
			//calculate horizontal fill
			//the maximum number of photos we can display horizontally without having less than Border pixels between
			int horizontal_fill = (m_Window.Width - Border) / (Border + m_PhotoSize);

			if (horizontal_fill < 1) horizontal_fill = 1;

			//the distance between successive centres
			float horizontal_space = (float)(m_Window.Width - 2 * Border - horizontal_fill * m_PhotoSize) / (horizontal_fill - 1);

			int xpos = pos % horizontal_fill;
			int ypos = pos / horizontal_fill;

			float x = Border + m_PhotoSize / 2.0f + xpos * (m_PhotoSize + horizontal_space);
			float y = (Border + m_PhotoSize) * ypos + Border + m_PhotoSize / 2.0f;
			return Matrix.Scaling(m_PhotoSize, m_PhotoSize, 0) * Matrix.Translation(-x, -y, 0);
		}

		private Matrix GetWorldTransform(int pos) {
			//calculate horizontal fill
			//the maximum number of photos we can display horizontally without having less than Border pixels between
			int horizontal_fill = GetHorizontalFill();

			if (horizontal_fill < 1) horizontal_fill = 1;

			int xpos = pos % horizontal_fill;
			int ypos = pos / horizontal_fill;

			float x = Border + m_PhotoSize / 2.0f + xpos * (m_PhotoSize + Border);
			float y = (Border + m_PhotoSize) * ypos + Border + m_PhotoSize / 2.0f;
			return Matrix.Scaling(m_PhotoSize, m_PhotoSize, 0) * Matrix.Translation(-x, -y, 0);
		}

		private Matrix GetZoomTransform() {
			return Matrix.Scaling(m_Window.Width, m_Window.Height, 0) * Matrix.Translation(-m_Window.Width / 2.0f, -m_ScrollPos - m_Window.Height / 2.0f, 1);
		}

		private void splitContainer1_Panel2_Resize(object sender, EventArgs e) {
			if (m_D3DDevice != null) {
				/*m_SwapChain.ResizeBuffers(2, m_Window.Width, m_Window.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
				m_SwapChain.ResizeTarget(new ModeDescription(m_Window.Width, m_Window.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm));
				Viewport viewport = new Viewport(0, 0, m_Window.Width, m_Window.Height, 0, 1);
				m_D3DDevice.Rasterizer.SetViewports(viewport);*/
			}
		}

		void MainForm_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
			if ((ModifierKeys & Keys.Control) == Keys.Control) {
				m_PhotoSize += 12 * e.Delta / 120;
				if (m_PhotoSize < 12) m_PhotoSize = 12;
			} else {
				m_ScrollPos -= e.Delta;
			}
		}

		public static float Lerp(float initial, float final, float factor) {
			return factor * final + (1 - factor) * initial;
		}

		private int GetHorizontalFill() {
			return (m_Window.Width - Border) / (Border + m_PhotoSize);
		}

		public int? GetMousePhoto(Point in_point) {
			int xpos, ypos;

			Point point = in_point;// Point.Subtract(in_point, new Size(m_Window.Location));

			if (point.X % (m_PhotoSize + Border) < Border) {
				//it's in the border area, invalid
				return null;
			} else {
				xpos = point.X / (m_PhotoSize + Border);
			}

			if ((point.Y + (int)m_ScrollPos) % (m_PhotoSize + Border) < Border) {
				//border
				return null;
			} else {
				ypos = (point.Y + (int)m_ScrollPos) / (m_PhotoSize + Border);
			}

			return ypos * GetHorizontalFill() + xpos;
		}

		private void splitContainer1_Panel2_MouseDoubleClick(object sender, MouseEventArgs e) {
			if (e.Button == System.Windows.Forms.MouseButtons.Left) {
				if (m_CurrentViewState is ZoomedViewState) {
					int photo = (m_CurrentViewState as ZoomedViewState).ZoomPhoto;
					m_CurrentViewState = new ZoomingViewState(false, photo, new NormalViewState(), new TimeSpan(3000000));
				} else {
					int? photo = GetMousePhoto(e.Location);
					if (photo.HasValue) {
						if (m_CurrentViewState is NormalViewState) {
							m_CurrentViewState = new ZoomingViewState(true, photo.Value, new ZoomedViewState(photo.Value), new TimeSpan(3000000));
						}
					}
				}
			}
		}
	}
}
