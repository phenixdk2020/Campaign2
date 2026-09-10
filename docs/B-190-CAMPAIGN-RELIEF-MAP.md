# B-190 — 3D kampagnekort / teater-relief

**Status:** FORSLAG / KANAL Campaign2  
**Projekt:** PROJECT 1864  
**Designanker:** Designmanual §4 og §23, backlog B-161 / B-168  
**Unity:** 6000.6.0f1  
**Dato:** 10. sep. 2026

Dette er **ikke** P0A battle-prototypen. Kortet er det strategiske hovedkort. Simulationen og visningen skilles ad.

## Problem

Danmark er for fladt og teatret for stort til ét Unity Terrain i 1:1. Et råt DEM ser ud som en grøn plade. Dybbøl, Als, Fredericia og Lillebælt — altså krigen — forsvinder. Moderne satellit/vejdata giver forkert 1864-geografi.

Designmanualen kræver allerede:

- 3D-reliefkort over Danmark, hertugdømmerne, Nordtyskland, Sverige-Norge og relevante søområder
- læsbart på nationsniveau **og** præcist nok til veje, jernbaner, færger, broer, floder og befæstninger
- intet synligt hex-grid
- bevægelse på navmesh/graf over terræn og infrastruktur
- regioner som administrative beholdere, ikke teleport-provinser
- synlig udvikling på kortet (B-168)

## Beslutning

**Ikke ét Danmark-Terrain. Et teater-relief med data under.**

### To skalaer, samme koordinater

| Lag | Rolle |
| --- | --- |
| Kampagne-relief | Styliseret 3D-mesh (Total War / Grand Tactician-retning). Overblik og operationel læsbarhed. |
| Transportgraf | Nodes/edges: byer, knudepunkter, broer, færger, stationer. Authoritative bevægelse. |
| Taktisk terræn | Senere, genereret fra **samme geo-data** (§23). Ikke en kopi af kampagnemeshen. |

En world-origin. Én meterskala. Enheder sidder på grafen og interpoleres langs edges. GameObjects er ikke save-data.

### Teater først, ikke hele riget

Første kort der skal se godt ud:

`Husum – Danevirke – Flensborg – Dybbøl – Als – Sønderborg – Kolding – Fredericia – Vejle + Lillebælt`

Sjælland, Fyn og København ligger som lavere-detalje flige, indtil flåde og politik har brug for dem.

### Højde

Rå DEM skal overdrives **8–15×**. Ellers læses moræner, ådale og Dybbøl-højderne ikke fra kampagnekameraet. Kyst og fjorde skæres skarpt. Vand er en rigtig 3D-flade med dybde i bælt og fjord — ikke blå tekstur på jorden.

### Infrastruktur er meshen

Veje, jernbane 1864, broer og færger er spline-meshes **på** terrænet og samtidig edges i bevægelsesgrafen. En forbedret vej skal kunne ses (B-168), og enheden skal faktisk gå på den (B-161). Ingen skjult `+20 % movement` fordi regionen er “god”.

### Look

Atlas-relief, ikke Google Earth.

- Shader: hældning + højde + landcover (mark, hede, mose, skov, klit, by)
- Byer og forter som læsbare 3D-klodser på 40 km kameraafstand
- Skov som klynger, ikke millioner af træer
- Vinter-1864 palet: dæmpet jord, mørk gran, gråt vand

## Unity-pipeline

Ikke Gaia/World Creator til hele landet.

1. Klip DEM + kyst + hydrografi (GST / Copernicus) over teatret.
2. Georeferér et 1860’er-kort ovenpå — ikke nutidens motorveje.
3. Eksportér tiles på ca. 25–40 km som 16-bit heightmap + masker.
4. Unity: én tile = mesh (foretrukket) eller Terrain til første forsøg.
5. Vector-lag (GeoJSON) → ScriptableObjects: byer, forter, broer, stationer, færger.
6. Graf-generator bygger nodes/edges. Det er simulationen. Meshen er visning.

Unity Terrain er acceptabelt til **Tile 00**. Den færdige kampagne bør være forbagte meshes + custom shader. Kyster og øer er nemmere sådan, og Lillebælt får ikke Terrain-huller.

## Må ikke

- Ét kæmpe Terrain over hele Danmark
- Synligt hex/provins-grid som bevægelse
- Fotorealistisk satellit
- Kampagnemeshen som den mesh, slaget også kæmpes på
- Moderne OSM-vejnet som 1864-infrastruktur

## Acceptance for kortet

Kortet duer når man på 10 sekunder kan pege på **Danevirke, Dybbøl, Als og Fredericia** og se hvorfor en hær sidder fast ved et bælt eller en bro.

Hvis det bare ligner “Danmark set skråt fra oven”, er det forkert — uanset opløsning.

## Relation til Strategy

P0A battle-prototypen og officer AI fortsætter i `Strategy`. Campaign2 ejer kort-data, tile-lister, shaders/notater og senere en isoleret map-scene. Fælles koordinatsystem dokumenteres her, før noget merges ind i Unity-roden.
