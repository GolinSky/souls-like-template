$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$equipmentRoot = $PSScriptRoot
$equipmentManifest = Get-Content -LiteralPath (Join-Path $equipmentRoot 'source-manifest.json') -Raw | ConvertFrom-Json
$equipmentResults = @()
foreach ($equipmentAsset in $equipmentManifest.assets) {
    $equipmentSource = [System.Drawing.Bitmap]::FromFile((Join-Path $equipmentRoot $equipmentAsset.source))
    foreach ($equipmentSize in @(256, 512)) {
        $equipmentBitmap = New-Object System.Drawing.Bitmap($equipmentSize, $equipmentSize, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $equipmentGraphics = [System.Drawing.Graphics]::FromImage($equipmentBitmap)
        $equipmentGraphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $equipmentGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $equipmentGraphics.Clear([System.Drawing.Color]::Transparent)
        $equipmentScale = if ($equipmentAsset.name -eq 'Arms') { 0.74 } else { 1.0 }
        $equipmentDrawSize = [int][Math]::Round($equipmentSize * $equipmentScale)
        $equipmentInset = [int][Math]::Round(($equipmentSize - $equipmentDrawSize) / 2)
        $equipmentGraphics.DrawImage($equipmentSource, $equipmentInset, $equipmentInset, $equipmentDrawSize, $equipmentDrawSize)
        $equipmentDestination = Join-Path $equipmentRoot ('Icons/EquipmentEmpty' + $equipmentAsset.name + '-' + $equipmentSize + '.png')
        $equipmentBitmap.Save($equipmentDestination, [System.Drawing.Imaging.ImageFormat]::Png)
        if ($equipmentSize -eq 256) {
            $equipmentLeft = 256; $equipmentTop = 256; $equipmentRight = -1; $equipmentBottom = -1
            $equipmentTransparent = 0
            for ($equipmentY = 0; $equipmentY -lt 256; $equipmentY++) {
                for ($equipmentX = 0; $equipmentX -lt 256; $equipmentX++) {
                    $equipmentAlpha = $equipmentBitmap.GetPixel($equipmentX, $equipmentY).A
                    if ($equipmentAlpha -eq 0) { $equipmentTransparent++ }
                    if ($equipmentAlpha -gt 16) {
                        $equipmentLeft = [Math]::Min($equipmentLeft, $equipmentX)
                        $equipmentTop = [Math]::Min($equipmentTop, $equipmentY)
                        $equipmentRight = [Math]::Max($equipmentRight, $equipmentX)
                        $equipmentBottom = [Math]::Max($equipmentBottom, $equipmentY)
                    }
                }
            }
            $equipmentResults += [pscustomobject]@{
                name = $equipmentAsset.name
                sourceCanvas = @($equipmentSource.Width, $equipmentSource.Height)
                exportCanvas = @(256, 256)
                semanticBoundsAlphaOver16 = @($equipmentLeft, $equipmentTop, ($equipmentRight - $equipmentLeft + 1), ($equipmentBottom - $equipmentTop + 1))
                transparentFraction = [Math]::Round($equipmentTransparent / 65536.0, 4)
                cornerAlpha = $equipmentBitmap.GetPixel(0, 0).A
                source = $equipmentAsset.source
                resizePolicy = 'Uniform full-canvas scaling; centered transparent padding for Arms at 74%'
                intendedCanvasInUI = @(64, 64)
                semanticSlotInUI = @(88, 88)
            }
        }
        $equipmentGraphics.Dispose()
        $equipmentBitmap.Dispose()
    }
    $equipmentSource.Dispose()
}
$equipmentResults | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $equipmentRoot 'icon-metadata.json') -Encoding utf8
$equipmentResults | Select-Object name, transparentFraction, cornerAlpha, semanticBoundsAlphaOver16 | ConvertTo-Json -Compress
