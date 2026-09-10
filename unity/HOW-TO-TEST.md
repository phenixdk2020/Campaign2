# campaign-map v00.00.02-atlas — test nu

Stilen er **staff-map / Grand Tactician-bord / HOI terrain-mode**, ikke satellit og ikke den gamle UNIFIED-DEM.

## Fil

`unity/TheaterAtlasBootstrap.cs`

Samme `Assets/Resources/CampaignMap/rings.txt` som v00.00.01.

## I Unity

1. Behold `rings.txt` i `Assets/Resources/CampaignMap/`
2. Kopiér `TheaterAtlasBootstrap.cs` til `Assets/Scripts/`
3. Fjern eller slå `TheaterSilhouetteBootstrap` fra
4. Tom GameObject → Add Component `TheaterAtlasBootstrap`
5. Play

Forventet console:
`[campaign-map v00.00.02-atlas] Parchment/ink Denmark.`

## Hvad der er nyt

- Pergament-land, blæk-vand, mørk kystlinje
- Lav bord-relief (ingen fake bjerge)
- Ingen Stockholm / Åbo
- Kamera mere HOI-skråt
- Byer kun i teatret

Det er stadig en prototype-shader, ikke GT's håndmalede tiles.
