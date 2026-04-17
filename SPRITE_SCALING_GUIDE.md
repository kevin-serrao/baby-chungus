# Sprite Scaling Guide for Baby Chungus

## Current Issues

- Attack sprites: 200 PPU (0.32 x 0.48 world units)
- Other sprites: 100 PPU (twice as large for same pixel dimensions)
- Inconsistent scaling across assets

## Recommended Scaling System

### 1. Choose Standard PPU

Set all sprites to **100 PPU** (pixels per unit) for consistency.

### 2. Define Reference Sizes

- **Player Character**: 1.0 x 1.5 world units (150 pixels tall at 100 PPU)
- **Enemies**: 0.8 x 1.2 world units
- **Environment**: Scale relative to player size

### 3. Scaling Methods

#### Method A: Transform Scale (Recommended)

- Keep all sprites at 100 PPU
- Use GameObject transform scale to adjust size
- Maintains consistent physics and collision

#### Method B: SpriteRenderer Size

- Set Draw Mode to "Simple"
- Adjust m_Size in SpriteRenderer
- Good for sprites that change size during animation

#### Method C: Pixels Per Unit Variation

- Only for sprites that need different "resolution" levels
- Use sparingly - can cause physics issues

## Implementation Steps

1. **Audit Current Assets**
   - Check pixel dimensions of all sprites
   - Calculate current world sizes
   - Identify inconsistencies

2. **Set Standard PPU**
   - Change attack sprites from 200 to 100 PPU
   - This will make them appear half size (0.16 x 0.24 units)

3. **Apply Transform Scaling**
   - Scale Player to desired size (e.g., 0.95x current scale)
   - Scale enemies proportionally
   - Use SpriteScaler component for precise control

4. **Test and Adjust**
   - Check gameplay balance
   - Ensure collisions work properly
   - Verify animations scale correctly

## Quick Fix for Current Setup

To standardize your current assets:

1. Change attack sprites PPU from 200 → 100
2. Scale Player transform by ~2x to compensate
3. Adjust camera orthographic size if needed
4. Test attack animation and collisions
