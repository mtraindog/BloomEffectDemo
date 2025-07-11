using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyCustomStuff;

public class BloomEffect
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _internalSpriteBatch;
    private RenderTarget2D _sceneTarget;
    private RenderTarget2D _bloomTarget1;
    private RenderTarget2D _bloomTarget2;
    private RenderTarget2D _bloomTarget3;
    private RenderTarget2D _bloomTarget4;    
    private readonly float _bloomIntensity;
    private readonly float _blurStrength;
    private readonly int _blurPasses;    
    private int _currentWidth;
    private int _currentHeight;
    
    public BloomEffect(GraphicsDevice graphicsDevice, 
        int width,
        int height,
        float bloomIntensity, 
        float blurStrength,
        int blurPasses)
    {
        _graphicsDevice = graphicsDevice;
        _internalSpriteBatch = new SpriteBatch(graphicsDevice);
        _bloomIntensity = bloomIntensity;
        _blurStrength = blurStrength;
        _blurPasses = blurPasses;
        
        // Initialize with current ParticleField bounds
        Resize(width, height);
    }
    
    public void Resize(int width, int height)
    {
        // Get current dimensions from ParticleField (scaled by 2 for the scale matrix)
        int scaledWidth = width;
        int scaledHeight = height;
        
        // Only recreate if dimensions actually changed
        if (scaledWidth != _currentWidth || scaledHeight != _currentHeight)
        {
            _currentWidth = scaledWidth;
            _currentHeight = scaledHeight;
            CreateRenderTargets(_currentWidth, _currentHeight);
        }
    }
    
    private void CreateRenderTargets(int width, int height)
    {
        // Dispose existing render targets
        DisposeRenderTargets();
        
        _sceneTarget = new RenderTarget2D(_graphicsDevice, width, height, false, 
            SurfaceFormat.Color, DepthFormat.None);
        _bloomTarget1 = new RenderTarget2D(_graphicsDevice, width / 2, height / 2, false, 
            SurfaceFormat.Color, DepthFormat.None);
        _bloomTarget2 = new RenderTarget2D(_graphicsDevice, width / 2, height / 2, false, 
            SurfaceFormat.Color, DepthFormat.None);
        _bloomTarget3 = new RenderTarget2D(_graphicsDevice, width / 4, height / 4, false, 
            SurfaceFormat.Color, DepthFormat.None);
        _bloomTarget4 = new RenderTarget2D(_graphicsDevice, width / 4, height / 4, false, 
            SurfaceFormat.Color, DepthFormat.None);
    }
    
    private void DisposeRenderTargets()
    {
        _sceneTarget?.Dispose();
        _bloomTarget1?.Dispose();
        _bloomTarget2?.Dispose();
        _bloomTarget3?.Dispose();
        _bloomTarget4?.Dispose();
        
        _sceneTarget = null;
        _bloomTarget1 = null;
        _bloomTarget2 = null;
        _bloomTarget3 = null;
        _bloomTarget4 = null;
    }
    
    public void BeginDraw(int width, int height)
    {
        // Ensure render targets match current dimensions
        Resize(width, height);
        
        _graphicsDevice.SetRenderTarget(_sceneTarget);
        _graphicsDevice.Clear(Color.Black);
    }
    
    public void EndDraw()
    {
        _graphicsDevice.SetRenderTarget(null);
        
        // Extract bright areas for bloom (downscale to 1/2)
        _graphicsDevice.SetRenderTarget(_bloomTarget1);
        _graphicsDevice.Clear(Color.Black);
        
        _internalSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
        _internalSpriteBatch.Draw(_sceneTarget, new Rectangle(0, 0, _bloomTarget1.Width, _bloomTarget1.Height), 
            Color.White);
        _internalSpriteBatch.End();
        
        // Downscale to 1/4 for blur
        _graphicsDevice.SetRenderTarget(_bloomTarget3);
        _graphicsDevice.Clear(Color.Black);
        
        _internalSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
        _internalSpriteBatch.Draw(_bloomTarget1, new Rectangle(0, 0, _bloomTarget3.Width, _bloomTarget3.Height), 
            Color.White);
        _internalSpriteBatch.End();
        
        // Apply multiple blur passes
        for (int pass = 0; pass < _blurPasses; pass++)
        {
            // Apply horizontal blur
            _graphicsDevice.SetRenderTarget(_bloomTarget4);
            _graphicsDevice.Clear(Color.Black);
            
            _internalSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            
            // Simple horizontal blur using multiple draws with offsets
            Color blurColor = new(1.0f, 1.0f, 1.0f, 0.2f);
            
            for (int i = -2; i <= 2; i++)
            {
                Vector2 offset = new(i * _blurStrength, 0);
                _internalSpriteBatch.Draw(_bloomTarget3, offset, blurColor);
            }
            
            _internalSpriteBatch.End();
            
            // Apply vertical blur
            _graphicsDevice.SetRenderTarget(_bloomTarget3);
            _graphicsDevice.Clear(Color.Black);
            
            _internalSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            
            // Simple vertical blur using multiple draws with offsets
            for (int i = -2; i <= 2; i++)
            {
                Vector2 offset = new(0, i * _blurStrength);
                _internalSpriteBatch.Draw(_bloomTarget4, offset, blurColor);
            }
            
            _internalSpriteBatch.End();
        }
        
        // Upscale blur back to 1/2
        _graphicsDevice.SetRenderTarget(_bloomTarget2);
        _graphicsDevice.Clear(Color.Black);
        
        _internalSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
        _internalSpriteBatch.Draw(_bloomTarget3, new Rectangle(0, 0, _bloomTarget2.Width, _bloomTarget2.Height), 
            Color.White);
        _internalSpriteBatch.End();
        
        // Combine scene with bloom using additive blending
        _graphicsDevice.SetRenderTarget(null);
        
        // Draw the original scene using internal SpriteBatch
        _internalSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, 
            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
        _internalSpriteBatch.Draw(_sceneTarget, Vector2.Zero, Color.White);
        _internalSpriteBatch.End();
        
        // Add bloom with additive blending using internal SpriteBatch
        _internalSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, 
            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
        _internalSpriteBatch.Draw(_bloomTarget2, new Rectangle(0, 0, _sceneTarget.Width, _sceneTarget.Height), 
            Color.White * _bloomIntensity);
        _internalSpriteBatch.End();
    }
    
    public void Dispose()
    {
        DisposeRenderTargets();
        _internalSpriteBatch?.Dispose();
    }
} 