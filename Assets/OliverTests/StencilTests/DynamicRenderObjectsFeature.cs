using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class DynamicRenderObjectsSettings
{
    [Header("Pass Settings")]
    public string passTag = "Dynamic Render Objects";
    public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;

    [Header("Filtering")]
    public LayerMask layerMask = ~0;

    [Header("Stencil Test")]
    public int stencilReference = 1;
    public CompareFunction stencilCompareFunction = CompareFunction.Equal;
}

public class DynamicRenderObjectsFeature : ScriptableRendererFeature
{
    public DynamicRenderObjectsSettings settings = new DynamicRenderObjectsSettings();

    class DynamicRenderObjectsPass : ScriptableRenderPass
    {
        private readonly DynamicRenderObjectsSettings settings;
        private FilteringSettings filteringSettings;
        private readonly ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");

        public DynamicRenderObjectsPass(DynamicRenderObjectsSettings settings)
        {
            this.settings = settings;
            renderPassEvent = settings.renderPassEvent;
            UpdateFiltering();
        }

        public void UpdateFiltering()
        {
            filteringSettings = new FilteringSettings(
                RenderQueueRange.all,
                settings.layerMask
            );
        }

        public void SetLayerMask(LayerMask mask)
        {
            settings.layerMask = mask;
            UpdateFiltering();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(settings.passTag);

            var drawingSettings = CreateDrawingSettings(
                shaderTagId,
                ref renderingData,
                SortingCriteria.CommonOpaque
            );

            // 🔑 Properly ENABLE stencil testing (this was the missing piece)
            var stencilState = new StencilState(
                enabled: true,
                readMask: 255,
                writeMask: 255,
                compareFunctionFront: settings.stencilCompareFunction,
                passOperationFront: StencilOp.Keep,
                failOperationFront: StencilOp.Keep,
                zFailOperationFront: StencilOp.Keep,
                compareFunctionBack: settings.stencilCompareFunction,
                passOperationBack: StencilOp.Keep,
                failOperationBack: StencilOp.Keep,
                zFailOperationBack: StencilOp.Keep
            );

            var renderStateBlock = new RenderStateBlock(RenderStateMask.Stencil)
            {
                stencilReference = settings.stencilReference,
                stencilState = stencilState
            };

            context.DrawRenderers(
                renderingData.cullResults,
                ref drawingSettings,
                ref filteringSettings,
                ref renderStateBlock
            );

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    DynamicRenderObjectsPass pass;

    public override void Create()
    {
        pass = new DynamicRenderObjectsPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }

    /// <summary>
    /// Runtime API — identical to swapping LayerMask on Render Objects
    /// </summary>
    public void SetLayerMask(LayerMask mask)
    {
        pass?.SetLayerMask(mask);
    }
}
