COMPLETE:
* [x] Tecles 0–9 per saltar directament al nivell corresponent
* [x] Es pot tornar al menú principal des del joc (tecla `Esc`) 

TODO:
* [ ] 3 pantalles: Menú, Jugar i Crèdits
  * [x] scena menu
  * [ ] scenas nivells 1-10
  * [ ] scena credits

* [x] Càmera ortogràfica orientada com al Bloxorz
* [ ] Fons de cada nivell no sòlid
* [x] Moviment/rotació del bloc amb WASD i fletxes
* [x] Si alguna part del bloc no està sustentada, cau i es repeteix el nivell
* [x] Si el bloc vertical entra al tile de destí, llisca dins i passa al següent nivell
* [x] Espai per canviar el cub controlat quan està dividit

* [x] Botons rodons (per contacte): activen ponts
* [x] Botons en creu (només en vertical)
* [x] Tiles de divisió: separen el bloc en dos; si es toquen, es recombinen
* [x] Tiles taronja: no sostenen el bloc vertical sencer; els cubs separats sí que hi poden passar

* [ ] 10 nivells de dificultat creixent (del Bloxorz original o propis)
* [ ] Sortides: guanyar (girar i pujar) i perdre (caure) com al Bloxorz
* [ ] HUD amb el total de moviments emprats per guanyar tots els nivells anteriors
* [ ] So i música
* [ ] Presentació del nivell amb tiles pujant des de baix


# Notas C#
- Si escrius una classe fora de `namespace X { }`, podras accedir a esa classe en tots els scripts
- Si es fa codi dintre de `namespace X { }`, s'utilitza `using X` per poder utilitzar-l'ho.
- Si alguna cosa pot ser nul·la es pot utilitzar `?`: `int? n;` i despres podem fer `n.HasValue` i `n.Value`.