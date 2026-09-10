# campaign-map v00.00.01-silhouette — test nu

Der var **ingen** ny Play-klar Unity-build før denne. Den gamle UNIFIED DENMARK-scene er uændret, indtil du lægger scriptet ind.

## Hvad det er

Silhuet-test. UTM32N. Danmark + Slesvig + en sliver af Skåne. Ingen Stockholm, ingen Åbo. Ikke relief, ikke 1864-kystfacit. Formålet er at se **Jylland, Fyn, Sjælland, Als**.

## Ind i Unity (campaign2-projektet)

1. Kopiér `TheaterSilhouetteBootstrap.cs` til `Assets/Scripts/`
2. Kopiér `Resources/CampaignMap/rings.txt` til `Assets/Resources/CampaignMap/rings.txt`
   Mappen **skal** hedde `Resources/CampaignMap/`
3. I den eksisterende kampagne-scene: Create Empty → navn `TheaterSilhouette`
4. Add Component → `TheaterSilhouetteBootstrap`
5. Slå den gamle UNIFIED DENMARK-mesh fra (scriptet slukker andre MeshRenderers som default)
6. Play

Forventet i Console:
`[campaign-map v00.00.01] Denmark+Slesvig silhouette klar.`

## Stop-test

Du skal på 2 sekunder kunne pege på Jylland, Fyn, Sjælland og Als. København på Sjælland. Flensborg/Dybbøl ved Slesvig.
