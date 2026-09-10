# Tile 00 — Sønderjylland / Slesvig-teatret

Første kampagnetile. Hvis den her ikke læses, er resten ligegyldigt.

## Omfang

Ca. 25–40 km-tiles der tilsammen dækker:

- Danevirke / Slesvig
- Flensborg og fjorden
- Dybbøl-stillingerne
- Als og Alssund
- Sønderborg
- Kolding
- Fredericia / brohoved
- Vejle ådal (nordkant)
- Lillebælt som sejlbar/færge-barriere

## Skal være synligt uden overlay

- Højderygge og ådale (overdrevet relief)
- Kystlinie og øer som rigtige silhuetter
- 1864-vejnet og jernbane som mesh på terrænet
- Bro- og færgepunkter som nodes
- Forter / byklodser: Slesvig, Flensborg, Sønderborg, Kolding, Fredericia

## Data (senere filer i `data/`)

| Lag | Formål |
| --- | --- |
| height_tile_00.png | 16-bit heightmap, overdrevet |
| mask_landcover.png | mark / hede / mose / skov / by / vand |
| coast.geojson | 1864-nær kyst |
| hydro.geojson | åer, fjord, bælt |
| roads_1864.geojson | chausséer og hovedveje |
| rail_1864.geojson | eksisterende baner |
| places.geojson | byer, forter, færger, broer |

## Unity-scene (planlagt)

`CampaignTheater_Tile00` — isoleret scene, ingen P0A battle-bootstrap. Kamera: skråt RTS/HOI-højde, ikke first person.

## Exit

1. Tile 00 renderer i Unity 6.6 uden at trække P0A med.
2. De fire nøglepunkter er genkendelige.
3. En dummy-hær kan stå på en edge Flensborg→Dybbøl.
4. Ingen moderne motorveje på meshen.
