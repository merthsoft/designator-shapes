param([string]$AtlasPath)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$repo=Split-Path -Parent $PSScriptRoot
$asset=Join-Path $repo 'Textures\UI\Commands\Smol\Additional'
New-Item -ItemType Directory -Force $asset,(Join-Path $asset 'Selected')|Out-Null
# Existing Halno assets: 40px, #CCC normal / #7A7A7A selected, black edge,
# near-transparent padding. Copy only the existing colored submenu badge.
function New-ShapeIcon([string]$kind,[bool]$selected) {
 $b=[Drawing.Bitmap]::new(40,40,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $g=[Drawing.Graphics]::FromImage($b);$g.Clear([Drawing.Color]::FromArgb(1,0,0,0));$g.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::None
 $v=if($selected){122}else{204};$color=[Drawing.Color]::FromArgb(255,$v,$v,$v)
 $pen=[Drawing.Pen]::new($color,3);$black=[Drawing.Pen]::new([Drawing.Color]::Black,7);$brush=[Drawing.SolidBrush]::new($color)
 $points=[Collections.Generic.List[Drawing.Point]]::new()
 switch($kind) {
  {$_ -in 'octagon','octagon_filled'} {
   foreach($p in @(@(13,5),@(26,5),@(34,13),@(34,26),@(26,34),@(13,34),@(5,26),@(5,13))){$points.Add([Drawing.Point]::new($p[0],$p[1]))}
   if($kind -eq 'octagon_filled'){$g.FillPolygon($brush,$points.ToArray())}else{$g.DrawPolygon($black,$points.ToArray());$g.DrawPolygon($pen,$points.ToArray())}
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
  'quarter_arc' {
   for($i=0;$i -le 60;$i++){$a=[Math]::PI/2*$i/60;$points.Add([Drawing.Point]::new([int][Math]::Round(6+27*[Math]::Cos($a)),[int][Math]::Round(33-27*[Math]::Sin($a))))}
   $g.DrawLines($black,$points.ToArray());$g.DrawLines($pen,$points.ToArray())
  }
  'semicircle_arc' {
   for($i=0;$i -le 120;$i++){$a=[Math]::PI*$i/120;$points.Add([Drawing.Point]::new([int][Math]::Round(19.5+15.5*[Math]::Cos($a)),[int][Math]::Round(29-15.5*[Math]::Sin($a))))}
   $g.DrawLines($black,$points.ToArray());$g.DrawLines($pen,$points.ToArray())
  }
 }
 $g.Dispose();$pen.Dispose();$black.Dispose();$brush.Dispose();return $b
}
$names='octagon','octagon_filled','checkerboard','checkerboard_inverse','l','l_inverse','quarter_arc','semicircle_arc'
foreach($name in $names){foreach($selected in $false,$true){$b=New-ShapeIcon $name $selected;$p=if($selected){Join-Path $asset ('Selected\'+$name+'.png')}else{Join-Path $asset ($name+'.png')};$b.Save($p,[Drawing.Imaging.ImageFormat]::Png);$b.Dispose()}}
$groups=@{octagons='octagon';checkerboards='checkerboard';corners='l';arcs='quarter_arc'}
foreach($group in $groups.Keys){foreach($state in 'open','close'){
 $b=New-ShapeIcon $groups[$group] $false;$g=[Drawing.Graphics]::FromImage($b)
 $reference=if($state -eq 'open'){'rectangle_more.png'}else{'rectangle_close.png'}
 $badge=[Drawing.Bitmap]::FromFile((Join-Path $repo ('Textures\UI\Commands\Smol\'+$reference)))
 $rect=[Drawing.Rectangle]::new(25,0,15,15);$g.DrawImage($badge,$rect,$rect,[Drawing.GraphicsUnit]::Pixel)
 $g.Dispose();$badge.Dispose();$b.Save((Join-Path $asset ($group+'_'+$state+'.png')),[Drawing.Imaging.ImageFormat]::Png);$b.Dispose()
}}
if($AtlasPath){
 $atlas=[Drawing.Bitmap]::new(760,($names.Count+4)*140+100);$g=[Drawing.Graphics]::FromImage($atlas);$g.Clear([Drawing.Color]::FromArgb(38,40,43));$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor;$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
 $font=[Drawing.Font]::new('Segoe UI',12);$g.DrawString('Designator Shapes icon QA - generated assets, not a game screenshot',$font,[Drawing.Brushes]::White,10,10)
 $rows=@();foreach($name in $names){$rows+=,@($name,(Join-Path $asset ($name+'.png')),(Join-Path $asset ('Selected\'+$name+'.png')))}
 foreach($group in @('octagons','checkerboards','corners','arcs')){$rows+=,@(($group+' open / close'),(Join-Path $asset ($group+'_open.png')),(Join-Path $asset ($group+'_close.png')))}
 $y=50;foreach($row in $rows){$g.DrawString($row[0],$font,[Drawing.Brushes]::White,10,$y+15);for($i=1;$i -le 2;$i++){$b=[Drawing.Bitmap]::FromFile($row[$i]);$x=210+($i-1)*265;$g.DrawImage($b,[Drawing.Rectangle]::new($x,$y+20,40,40));$g.DrawImage($b,[Drawing.Rectangle]::new($x+60,$y,120,120));$b.Dispose()};$y+=140}
 $g.Dispose();$font.Dispose();$atlas.Save($AtlasPath,[Drawing.Imaging.ImageFormat]::Png);$atlas.Dispose()
}
