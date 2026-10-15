param([string]$AtlasPath)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$repo=Split-Path -Parent $PSScriptRoot
$asset=Join-Path $repo 'Textures\UI\Commands\Smol\Additional'
New-Item -ItemType Directory -Force $asset,(Join-Path $asset 'Selected')|Out-Null
# Existing Halno assets: 40px, #CCC normal / #7A7A7A selected, black edge,
# fully transparent padding. Extract native badge colored pixels plus their exact two-pixel black border.
function New-ShapeIcon([string]$kind,[bool]$selected) {
 $b=[Drawing.Bitmap]::new(40,40,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[Drawing.Graphics]::FromImage($b);$g.Clear([Drawing.Color]::Transparent);$g.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::None
 $v=if($selected){122}else{204};$color=[Drawing.Color]::FromArgb(255,$v,$v,$v)
 $pen=[Drawing.Pen]::new($color,3);$black=[Drawing.Pen]::new([Drawing.Color]::Black,7);$brush=[Drawing.SolidBrush]::new($color)
 $points=[Collections.Generic.List[Drawing.Point]]::new()
 $baseKind=$kind.Replace('midpoint_','')
 switch($baseKind) {
  {$_ -in 'octagon','octagon_filled'} {
   foreach($p in @(@(13,5),@(26,5),@(34,13),@(34,26),@(26,34),@(13,34),@(5,26),@(5,13))){$points.Add([Drawing.Point]::new($p[0],$p[1]))}
   if($baseKind -eq 'octagon_filled'){$g.FillPolygon($brush,$points.ToArray())}else{$g.DrawPolygon($black,$points.ToArray());$g.DrawPolygon($pen,$points.ToArray())}
  }
  {$_ -in 'checkerboard','checkerboard_inverse'} {
   $phase=if($kind -eq 'checkerboard'){0}else{1}
   $g.FillRectangle([Drawing.Brushes]::Black,3,3,34,34)
   for($y=0;$y -lt 4;$y++){for($x=0;$x -lt 4;$x++){if((($x+3-$y)%2) -eq $phase){$g.FillRectangle($brush,5+$x*8,5+$y*8,7,7)}}}
  }
  {$_ -in 'l','l_inverse'} {
   $coords=if($kind -eq 'l'){@(@(7,5),@(7,32),@(34,32))}else{@(@(5,7),@(32,7),@(32,34))}
   foreach($p in $coords){$points.Add([Drawing.Point]::new($p[0],$p[1]))};$g.DrawLines($black,$points.ToArray());$g.DrawLines($pen,$points.ToArray())
  }
  {$_ -in 'quarter_arc','semicircle_arc'} {
   # Arc glyphs are exact native-scale sections of the original circle pixel art.
   # Do not rebuild curves from overlapping thick polylines or scale their stroke.
   $suffix=if($selected){'Selected\Grayscale\'}else{''}
   $reference=[Drawing.Bitmap]::FromFile((Join-Path $repo ('Textures\UI\Commands\Smol\'+$suffix+'circle.png')))
   $left=if($baseKind -eq 'quarter_arc'){20}else{0}
   $destX=if($baseKind -eq 'quarter_arc'){9}else{0}
   $destY=if($baseKind -eq 'quarter_arc'){9}else{8}
   for($y=0;$y -lt 20;$y++){for($x=$left;$x -lt 40;$x++){
    $c=$reference.GetPixel($x,$y)
    if($c.A -gt 1){$b.SetPixel($destX+$x-$left,$destY+$y,$c)}
   }}
   $reference.Dispose()
  }
 }
 $g.Dispose();$pen.Dispose();$black.Dispose();$brush.Dispose()
 if($kind.StartsWith('midpoint_')){
  $suffix=if($selected){'Selected\Grayscale\'}else{''}
  $referenceName=if($baseKind -eq 'octagon_filled'){'hexagon_filled'}else{'hexagon'}
  $reference=[Drawing.Bitmap]::FromFile((Join-Path $repo ('Textures\UI\Commands\Smol\'+$suffix+$referenceName+'_midpoint.png')))
  $baseline=[Drawing.Bitmap]::FromFile((Join-Path $repo ('Textures\UI\Commands\Smol\'+$suffix+$referenceName+'.png')))
  for($y=16;$y -le 23;$y++){for($x=16;$x -le 23;$x++){$c=$reference.GetPixel($x,$y);if($c.A -gt 10 -and $c.ToArgb() -ne $baseline.GetPixel($x,$y).ToArgb()){$b.SetPixel($x,$y,$c)}}}
  $reference.Dispose();$baseline.Dispose()
 }
 return $b
}
$names='octagon','octagon_filled','midpoint_octagon','midpoint_octagon_filled','checkerboard','checkerboard_inverse','l','l_inverse','quarter_arc','semicircle_arc'
foreach($name in $names){foreach($selected in $false,$true){$b=New-ShapeIcon $name $selected;$p=if($selected){Join-Path $asset ('Selected\'+$name+'.png')}else{Join-Path $asset ($name+'.png')};$b.Save($p,[Drawing.Imaging.ImageFormat]::Png);$b.Dispose()}}
$groups=@{octagons='octagon';checkerboards='checkerboard';arcs='quarter_arc'}
foreach($group in $groups.Keys){foreach($state in 'open','close'){
 $b=New-ShapeIcon $groups[$group] $false
 $reference=if($state -eq 'open'){'rectangle_more.png'}else{'rectangle_close.png'}
 $badge=[Drawing.Bitmap]::FromFile((Join-Path $repo ('Textures\UI\Commands\Smol\'+$reference)))
 $mask=[Collections.Generic.HashSet[string]]::new()
 for($y=0;$y -lt 40;$y++){for($x=0;$x -lt 40;$x++){
  $c=$badge.GetPixel($x,$y)
  $colored=$c.A -gt 10 -and (($state -eq 'open' -and $c.G -gt 2*$c.R -and $c.G -gt 2*$c.B) -or ($state -eq 'close' -and $c.R -gt 2*$c.G -and $c.R -gt 2*$c.B))
  if($colored){for($dy=-2;$dy -le 2;$dy++){for($dx=-2;$dx -le 2;$dx++){[void]$mask.Add("$($x+$dx),$($y+$dy)")}}}
 }}
 # Reserve a transparent badge rectangle, not a cropped source rectangle.
 # This removes geometry fragments in the voids of the plus as well as around it.
 $coords=@($mask|ForEach-Object {$xy=$_.Split(',');[pscustomobject]@{X=[int]$xy[0];Y=[int]$xy[1]}})
 $left=($coords.X|Measure-Object -Minimum).Minimum-1;$right=($coords.X|Measure-Object -Maximum).Maximum+1
 $top=($coords.Y|Measure-Object -Minimum).Minimum-1;$bottom=($coords.Y|Measure-Object -Maximum).Maximum+1
 for($y=[Math]::Max(0,$top);$y -le [Math]::Min(39,$bottom);$y++){for($x=[Math]::Max(0,$left);$x -le [Math]::Min(39,$right);$x++){$b.SetPixel($x,$y,[Drawing.Color]::Transparent)}}
 foreach($pixel in $mask){$xy=$pixel.Split(',');$x=[int]$xy[0];$y=[int]$xy[1];if($x -ge 0 -and $y -ge 0 -and $x -lt 40 -and $y -lt 40){   $c=$badge.GetPixel($x,$y)
   $colored=($state -eq 'open' -and $c.G -gt 2*$c.R -and $c.G -gt 2*$c.B) -or ($state -eq 'close' -and $c.R -gt 2*$c.G -and $c.R -gt 2*$c.B)
   # A dilation may touch the reference rectangle. Never copy its gray body.
   if($c.A -gt 10 -and ($colored -or [Math]::Max($c.R,[Math]::Max($c.G,$c.B)) -le 32)){$b.SetPixel($x,$y,$c)}}}
 $badge.Dispose();$b.Save((Join-Path $asset ($group+'_'+$state+'.png')),[Drawing.Imaging.ImageFormat]::Png);$b.Dispose()
}}
if($AtlasPath){
 foreach($light in @($false,$true)){
 $atlas=[Drawing.Bitmap]::new(760,($names.Count+3)*140+100);$g=[Drawing.Graphics]::FromImage($atlas);$g.Clear($(if($light){[Drawing.Color]::FromArgb(245,245,245)}else{[Drawing.Color]::FromArgb(38,40,43)}));$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor;$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
 $textBrush=if($light){[Drawing.Brushes]::Black}else{[Drawing.Brushes]::White};$font=[Drawing.Font]::new('Segoe UI',12);$g.DrawString('Designator Shapes icon QA - generated assets, not a game screenshot',$font,$textBrush,10,10)
 $rows=@();foreach($name in $names){$rows+=,@($name,(Join-Path $asset ($name+'.png')),(Join-Path $asset ('Selected\'+$name+'.png')))}
 foreach($group in @('octagons','checkerboards','arcs')){$rows+=,@(($group+' open / close'),(Join-Path $asset ($group+'_open.png')),(Join-Path $asset ($group+'_close.png')))}
 $y=50;foreach($row in $rows){$g.DrawString($row[0],$font,$textBrush,10,$y+15);for($i=1;$i -le 2;$i++){$b=[Drawing.Bitmap]::FromFile($row[$i]);$x=210+($i-1)*265;$g.DrawImage($b,[Drawing.Rectangle]::new($x,$y+20,40,40));$g.DrawImage($b,[Drawing.Rectangle]::new($x+60,$y,120,120));$b.Dispose()};$y+=140}
 $g.Dispose();$font.Dispose();$output=if($light){[IO.Path]::Combine([IO.Path]::GetDirectoryName($AtlasPath),[IO.Path]::GetFileNameWithoutExtension($AtlasPath)+'-light.png')}else{$AtlasPath};$atlas.Save($output,[Drawing.Imaging.ImageFormat]::Png);$atlas.Dispose()
}

}
