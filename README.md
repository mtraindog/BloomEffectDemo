# BloomEffect

A high-performance bloom post-processing effect implementation for MonoGame/XNA applications. This effect adds a beautiful bloom glow to bright areas of your scene, enhancing visual appeal and creating more atmospheric lighting.

## Features

- **Real-time bloom post-processing** - Adds glow effects to bright areas
- **Configurable parameters** - Adjust bloom intensity, blur strength, and blur passes
- **Multi-pass blur** - High-quality gaussian-like blur using multiple passes
- **Dynamic resizing** - Automatically handles resolution changes
- **Memory efficient** - Uses multiple render targets with proper disposal
- **Additive blending** - Combines bloom with original scene using additive blending

## Requirements

- MonoGame 3.8+ or XNA Framework
- .NET 6.0+ (for modern MonoGame versions)
- Graphics device with support for render targets

## Installation

1. Copy `BloomEffect.cs` into your project
2. Add the necessary using statements to your game class:
   ```csharp
   using Microsoft.Xna.Framework;
   using Microsoft.Xna.Framework.Graphics;
   ```

## Usage

### Basic Setup

```csharp
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private BloomEffect _bloomEffect;
    private SpriteBatch _spriteBatch;
    
    protected override void Initialize()
    {
        _graphics = new GraphicsDeviceManager(this);
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Initialize bloom effect with your desired parameters
        _bloomEffect = new BloomEffect(
            graphicsDevice: GraphicsDevice,
            width: _graphics.PreferredBackBufferWidth,
            height: _graphics.PreferredBackBufferHeight,
            bloomIntensity: 0.8f,    // How strong the bloom effect is (0.0 - 1.0)
            blurStrength: 2.0f,      // How much blur to apply
            blurPasses: 3            // Number of blur passes (more = smoother but slower)
        );
        
        base.Initialize();
    }
}
```

### Rendering with Bloom

```csharp
protected override void Draw(GameTime gameTime)
{
    // Begin bloom rendering
    _bloomEffect.BeginDraw(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
    
    // Draw your scene here - everything will be captured for bloom processing
    GraphicsDevice.Clear(Color.Black);
    
    // Example: Draw some bright sprites that will bloom
    _spriteBatch.Begin();
    _spriteBatch.Draw(yourBrightTexture, position, Color.White);
    _spriteBatch.End();
    
    // End bloom rendering - this will apply the bloom effect and draw to screen
    _bloomEffect.EndDraw();
    
    base.Draw(gameTime);
}
```

### Handling Window Resize

```csharp
protected override void LoadContent()
{
    // Your content loading code...
    
    // Handle window resize
    Window.ClientSizeChanged += (sender, e) =>
    {
        _bloomEffect.Resize(Window.ClientBounds.Width, Window.ClientBounds.Height);
    };
}
```

### Cleanup

```csharp
protected override void UnloadContent()
{
    _bloomEffect?.Dispose();
    _spriteBatch?.Dispose();
    base.UnloadContent();
}
```

## Parameters

### BloomIntensity (0.0 - 1.0)
Controls how strong the bloom effect appears. Higher values create more pronounced glow effects.

- `0.0` - No bloom effect
- `0.5` - Moderate bloom
- `1.0` - Maximum bloom intensity

### BlurStrength (1.0 - 5.0)
Controls the spread of the blur effect. Higher values create softer, more diffused blooms.

- `1.0` - Sharp bloom
- `2.0` - Moderate blur (recommended)
- `5.0` - Very soft, diffused bloom

### BlurPasses (1 - 5)
Number of blur iterations. More passes create smoother, higher-quality blur but impact performance.

- `1` - Fast, basic blur
- `3` - Good quality (recommended)
- `5` - High quality, slower performance

## Performance Considerations

- **Render Targets**: The effect uses multiple render targets which consume GPU memory
- **Blur Passes**: Each additional blur pass increases GPU load
- **Resolution**: Higher resolutions require more processing power
- **Memory**: Render targets are automatically disposed when resizing or disposing

## Tips for Best Results

1. **Bright Areas**: Draw bright sprites/particles that you want to bloom
2. **Dark Backgrounds**: Bloom effects work best on dark backgrounds
3. **Color Balance**: Use additive blending for natural-looking bloom
4. **Performance**: Start with 2-3 blur passes and adjust based on performance
5. **Intensity**: 0.6-0.8 bloom intensity usually looks best

## Example Scene Setup

```csharp
// Draw a dark background
GraphicsDevice.Clear(Color.DarkBlue);

// Draw bright particles/sprites that will bloom
_spriteBatch.Begin();
foreach (var particle in brightParticles)
{
    _spriteBatch.Draw(particleTexture, particle.Position, 
        Color.White * particle.Brightness);
}
_spriteBatch.End();
```

## License

This project is open source and available under the MIT License.

## Contributing

Feel free to submit issues, feature requests, or pull requests to improve the bloom effect implementation. 