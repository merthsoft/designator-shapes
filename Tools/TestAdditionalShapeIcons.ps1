$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$repo=Split-Path -Parent $PSScriptRoot
$root=Join-Path $repo 'Textures\UI\Commands\Smol'
$asset=Join-Path $root 'Additional'
$script:checks=0
function Assert-Pixel($ok,$message){$script:checks++;if(!$ok){throw $message}}
foreach($file in Get-ChildItem $asset -Recurse -Filter '*.png'){
 $b=[Drawing.Bitmap]::FromFile($file.FullName)
 Assert-Pixel ($b.Width -eq 40 -and $b.Height -eq 40) "$($file.Name) dimensions"
 for($y=0;$y -lt 40;$y++){for($x=0;$x -lt 40;$x++){Assert-Pixel ($b.GetPixel($x,$y).A -ne 1) "$($file.Name) near-transparent background pixel"}}
 foreach($xy in @(@(0,0),@(39,0),@(0,39),@(39,39))){Assert-Pixel ($b.GetPixel($xy[0],$xy[1]).A -eq 0) "$($file.Name) opaque corner"}
 $b.Dispose()
}
foreach($selected in $false,$true){foreach($kind in 'quarter_arc','semicircle_arc'){
 $suffix=if($selected){'Selected\'}else{''};$referenceSuffix=if($selected){'Selected\Grayscale\'}else{''}
 $b=[Drawing.Bitmap]::FromFile((Join-Path $asset ($suffix+$kind+'.png')))
 $source=[Drawing.Bitmap]::FromFile((Join-Path $root ($referenceSuffix+'circle.png')))
 $left=if($kind -eq 'quarter_arc'){20}else{0};$destX=if($kind -eq 'quarter_arc'){9}else{0};$destY=if($kind -eq 'quarter_arc'){9}else{8}
 for($y=0;$y -lt 40;$y++){for($x=0;$x -lt 40;$x++){
  $expected=[Drawing.Color]::Transparent
  if($x -ge $destX -and $x -lt $destX+40-$left -and $y -ge $destY -and $y -lt $destY+20){$c=$source.GetPixel($left+$x-$destX,$y-$destY);if($c.A -gt 1){$expected=$c}}
  $actual=$b.GetPixel($x,$y)
  Assert-Pixel (($actual.A -eq 0 -and $expected.A -eq 0) -or $actual.ToArgb() -eq $expected.ToArgb()) "$suffix$kind curve differs from native circle pixels"
 }};$b.Dispose();$source.Dispose()
}}
foreach($group in @('octagons','checkerboards','arcs')){foreach($state in 'open','close'){
 $b=[Drawing.Bitmap]::FromFile((Join-Path $asset ($group+'_'+$state+'.png')))
 $sourceName=if($state -eq 'open'){'rectangle_more.png'}else{'rectangle_close.png'}
 $source=[Drawing.Bitmap]::FromFile((Join-Path $root $sourceName));$mask=[Collections.Generic.HashSet[string]]::new()
 for($y=0;$y -lt 40;$y++){for($x=0;$x -lt 40;$x++){$c=$source.GetPixel($x,$y);if($c.A -gt 10 -and (($state -eq 'open' -and $c.G -gt 2*$c.R -and $c.G -gt 2*$c.B) -or ($state -eq 'close' -and $c.R -gt 2*$c.G -and $c.R -gt 2*$c.B))){for($dy=-2;$dy -le 2;$dy++){for($dx=-2;$dx -le 2;$dx++){[void]$mask.Add("$($x+$dx),$($y+$dy)")}}}}}
  foreach($pixel in $mask){
  $xy=$pixel.Split(',');$actual=$b.GetPixel([int]$xy[0],[int]$xy[1]);$c=$source.GetPixel([int]$xy[0],[int]$xy[1])
  $colored=($state -eq 'open' -and $c.G -gt 2*$c.R -and $c.G -gt 2*$c.B) -or ($state -eq 'close' -and $c.R -gt 2*$c.G -and $c.R -gt 2*$c.B)
  $copy=$c.A -gt 10 -and ($colored -or [Math]::Max($c.R,[Math]::Max($c.G,$c.B)) -le 32)
  Assert-Pixel (($copy -and $actual.ToArgb() -eq $c.ToArgb()) -or (!$copy -and $actual.A -eq 0)) "$group $state badge copied unrelated reference pixels"
 }
 foreach($pixel in $mask){$xy=$pixel.Split(',');for($dy=-1;$dy -le 1;$dy++){for($dx=-1;$dx -le 1;$dx++){$x=[int]$xy[0]+$dx;$y=[int]$xy[1]+$dy;if($x -ge 0 -and $y -ge 0 -and $x -lt 40 -and $y -lt 40 -and !$mask.Contains("$x,$y")){Assert-Pixel ($b.GetPixel($x,$y).A -eq 0) "$group $state stray badge surroundings"}}}}
 $b.Dispose();$source.Dispose()
}}
Write-Output "PASS $script:checks icon pixel/dimension/transparency assertions"
