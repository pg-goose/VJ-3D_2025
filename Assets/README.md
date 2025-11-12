* [ ] 3 pantalles: Menú, Jugar i Crèdits
* [ ] Càmera ortogràfica orientada com al Bloxorz
* [ ] Fons de cada nivell no sòlid
* [x] Moviment/rotació del bloc amb WASD i fletxes
* [ ] Si alguna part del bloc no està sustentada, cau i es repeteix el nivell
* [ ] Si el bloc vertical entra al tile de destí, llisca dins i passa al següent nivell
* [ ] Es pot tornar al menú principal des del joc
* [ ] Espai per canviar el cub controlat quan està dividit

* [ ] Botons rodons (per contacte): activen ponts
* [ ] Botons en creu (només en vertical)
* [ ] Tiles de divisió: separen el bloc en dos; si es toquen, es recombinen
* [ ] Tiles taronja: no sostenen el bloc vertical sencer; els cubs separats sí que hi poden passar

* [ ] 10 nivells de dificultat creixent (del Bloxorz original o propis)
* [ ] Tecles 0–9 per saltar directament al nivell corresponent
* [ ] Sortides: guanyar (girar i pujar) i perdre (caure) com al Bloxorz

* [ ] HUD amb el total de moviments emprats per guanyar tots els nivells anteriors
* [ ] So i música
* [ ] Presentació del nivell amb tiles pujant des de baix


# Notas C#
- Si escrius una classe fora de `namespace X { }`, podras accedir a esa classe en tots els scripts
- Si es fa codi dintre de `namespace X { }`, s'utilitza `using X` per poder utilitzar-l'ho.
- Si alguna cosa pot ser nul·la es pot utilitzar `?`: `int? n;` i despres podem fer `n.HasValue` i `n.Value`.