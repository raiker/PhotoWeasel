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
		Effect m_Effect;
		SlimDX.Direct3D10.Buffer m_Vertices;
		Texture2D m_Tex;

		ScrollableControl m_Window;

		const int InputStride = 24;

		public MainForm() {
			InitializeComponent();
			m_Window = splitContainer1.Panel2;
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

				m_D3DDevice.OutputMerger.SetTargets(m_RenderTargetView);
			}

			{
				Viewport viewport = new Viewport(0, 0, m_Window.Width, m_Window.Height, 0, 1);
				m_D3DDevice.Rasterizer.SetViewports(viewport);
			}

			m_Effect = Effect.FromFile(m_D3DDevice, "effect.fx", "fx_4_0");

			m_Tex = Texture2D.FromFile(m_D3DDevice, "IMG_4788.jpg");

			GenerateGeometry();
		}

		private void GenerateGeometry() {
			using (var stream = new DataStream(4 * InputStride, true, true)) {
				for (int i = 0; i < 4; i++) {
					float x = (i % 2 == 0) ? -0.5f : 0.5f;
					float y = (i / 2 == 0) ? -0.5f : 0.5f;
					stream.Write(new Vector4(x, 0.0f, y, 1.0f));
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

				m_Effect.GetVariableByName("World").AsMatrix().SetMatrix(Matrix.Scaling(512, 0, 512));
				m_Effect.GetVariableByName("View").AsMatrix().SetMatrix(Matrix.LookAtLH(new Vector3(0, -4, 0), new Vector3(0, 0, 0), Vector3.UnitZ));
				m_Effect.GetVariableByName("Projection").AsMatrix().SetMatrix(Matrix.OrthoLH(m_Window.Width, m_Window.Height, 1, 10));

				m_D3DDevice.InputAssembler.SetInputLayout(layout);

				ShaderResourceView resource_view = new ShaderResourceView(m_D3DDevice, m_Tex);

				m_Effect.GetVariableByName("tex2D").AsResource().SetResource(resource_view);
				m_Effect.GetVariableByName("AspectRatio").AsScalar().Set((float)m_Tex.Description.Width / m_Tex.Description.Height);

				pass.Apply();
				m_D3DDevice.Draw(4, 0);
			}

			m_SwapChain.Present(1, PresentFlags.None);
		}

		private void splitContainer1_Panel2_Resize(object sender, EventArgs e) {
			if (m_D3DDevice != null) {
				m_SwapChain.ResizeBuffers(2, m_Window.Width, m_Window.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
				m_SwapChain.ResizeTarget(new ModeDescription(m_Window.Width, m_Window.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm));
				Viewport viewport = new Viewport(0, 0, m_Window.Width, m_Window.Height, 0, 1);
				m_D3DDevice.Rasterizer.SetViewports(viewport);
			}
		}
	}
}
