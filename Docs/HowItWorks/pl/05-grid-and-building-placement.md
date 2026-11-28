# 05 · Siatka i stawianie budynków

**W jednym zdaniu:** teren kolonii jest podzielony na niewidoczne kwadratowe komórki, każdy budynek zajmuje całkowitą liczbę komórek, a wszystkie budynki stoją na jednej stałej wysokości, zwanej poziomem zabudowy.

**Stan:** prototyp. Budynki są pokazywane jako zwykłe pudełka, nic nie kosztują i nie można ich jeszcze usuwać, obracać ani zapisywać.

## Po co nam to

Budynki nie mogą na siebie nachodzić (GDD §3), a każdy ma stały rozmiar. Sprawdzanie tego przy dowolnych pozycjach i kształtach modeli byłoby wolne i nieprzewidywalne. Siatka zamienia pytanie „czy te dwa budynki się zderzają?” na pytanie „czy któraś z tych komórek jest już zajęta?”, a to proste sprawdzenie w tabeli.

Teren nie jest płaski, ale nasze budynki są modelowane na betonowych płytach i palach. Żeby każdy model pasował w każdym miejscu, wszystkie budynki stoją dokładnie na tej samej wysokości. Płyta albo pale modelu sięgają w dół do nierównego gruntu.

## Jak to działa

1. **Siatka.** Obszar gry ma 100 × 100 komórek, każda 10 m × 10 m (łącznie 1 km × 1 km). Siatka jest wyśrodkowana na środku mapy. Komórka (0, 0) to narożnik o najmniejszych wartościach X i Z; kolumny biegną wzdłuż osi X, wiersze wzdłuż osi Z. Siatka nigdy nie jest rysowana: gracz widzi tylko, jak podgląd budynku przeskakuje z komórki na komórkę.
2. **Obrys budynku.** Każdy typ budynku określa, ile komórek zajmuje, na przykład 2 kolumny × 2 wiersze. To jego obrys. Silos (w danych gry: Silo) ma 1 × 1, budynek mieszkalny (Residential Building) 2 × 2, fabryka (Factory) 3 × 2. To rozmiary tymczasowe.
3. **Przyciąganie do siatki.** Gdy gracz wskazuje miejsce na ziemi, szukamy obrysu, którego środek leży najbliżej wskaźnika. Budynek 1 × 1 jest wyśrodkowany w komórce pod wskaźnikiem. Budynek 2 × 2 jest wyśrodkowany w najbliższym narożniku, w którym stykają się cztery komórki. Każda oś jest przyciągana osobno, więc budynek 3 × 2 jest wzdłuż osi X wyśrodkowany w komórce, a wzdłuż osi Z na granicy komórek.
4. **Sprawdzanie.** Obrys jest sprawdzany w dwóch krokach: po pierwsze, czy wszystkie jego komórki mieszczą się w siatce; po drugie, czy każda z nich jest wolna. Odpowiedź to jedna z trzech wartości: dozwolone, poza siatką, zajęte.
5. **Stawianie.** Jeśli odpowiedź brzmi „dozwolone”, komórki zostają oznaczone jako zajęte, budynek trafia na listę postawionych budynków, a zdarzenie ogłasza to, żeby scena mogła go pokazać.
6. **Poziom zabudowy.** Pozycja na siatce to tylko dwie liczby (X i Z). Wysokość nie jest jej częścią: to jedna wartość dla całej mapy, przechowywana w ustawieniach siatki. Żadna reguła nie może nadać budynkowi innej wysokości.

Przykład: wskaźnik znajduje się 1 m na wschód i 1 m na północ od środka mapy. Dla silosu (1 × 1) wynikiem jest komórka (50, 50), której środek leży 5 m na wschód i 5 m na północ od środka mapy. Dla budynku mieszkalnego (2 × 2) wynikiem są komórki od (49, 49) do (50, 50), wyśrodkowane dokładnie na środku mapy.

### Najważniejsze klasy

| Klasa | Zadanie |
| --- | --- |
| `BuildGridSettings` (dane) | Rozmiar komórki, liczba kolumn i wierszy, wysokość poziomu zabudowy. |
| `BuildingDefinition` (dane) | Jeden typ budynku: identyfikator, nazwa, obrys w komórkach. |
| `GridCell` | Adres jednej komórki: kolumna i wiersz. |
| `GridArea` | Prostokąt komórek: obrys budynku w konkretnym miejscu. |
| `GroundPoint` | Pozycja na poziomie zabudowy w metrach (tylko X i Z). |
| `BuildGridGeometry` | Przelicza metry na komórki (przyciąganie do siatki) i komórki z powrotem na metry. |
| `BuildGrid` | Pamięta, które komórki są zajęte; odpowiada: dozwolone / poza siatką / zajęte. |
| `BuildingRegistry` | Lista postawionych budynków; jedyny sposób dodania budynku; wywołuje `OnBuildingPlaced`. |
| `PlacedBuilding` | Jeden budynek na siatce: jego typ i jego komórki. |

Wszystkie te klasy, poza dwiema klasami danych, to zwykły C# bez żadnego związku ze sceną, więc można je testować bez wciskania Play (zob. 02).

### W scenie: wskazywanie, podgląd i kamera

1. **Na co wskazuje mysz.** Prowadzimy wyobrażoną linię prostą (promień) od kamery przez wskaźnik myszy. Nie pytamy, gdzie ta linia trafia w teren. Pytamy, gdzie przecina płaską, poziomą, niewidoczną płaszczyznę na wysokości poziomu zabudowy. To cała „blokada wysokości”: wzniesienia i zagłębienia terenu nie mogą zmienić wyniku, a teren nie potrzebuje kształtu kolizji na potrzeby budowania.
2. **Podgląd.** Punkt przecięcia jest przyciągany do komórek (krok 3 powyżej) i w tym miejscu pojawia się pudełko podglądu, stojące na poziomie zabudowy. Jest zielone, gdy reguły odpowiadają „dozwolone”, a czerwone w pozostałych przypadkach (poza siatką albo zajęte).
3. **Stawianie.** Kliknięcie lewym przyciskiem myszy prosi reguły o postawienie wybranego budynku. Jeśli się zgodzą, w tym miejscu pojawia się szare pudełko. Klawisze od 1 do 3 wybierają budynek: silos, budynek mieszkalny, fabrykę.
4. **Kamera.** Kamera patrzy pod stałym kątem w dół, na punkt leżący na poziomie zabudowy. Klawisze W, A, S, D albo strzałki przesuwają ten punkt, a kółko myszy zmienia odległość. Punkt nie może wyjść poza siatkę.
5. **Kto co łączy.** Jeden obiekt w scenie, `GameBootstrap`, przy starcie sceny tworzy siatkę, reguły i sterowanie, a potem przekazuje je kamerze, kontrolerowi stawiania budynków i widokowi. Żaden skrypt nie szuka innego skryptu.

| Klasa (wszystkie w `Colony.Game`) | Zadanie |
| --- | --- |
| `GameBootstrap` | Tworzy wszystko przy starcie sceny i łączy części ze sobą. |
| `PrototypeControls` | Klawisze i przyciski myszy używane w prototypie, odczytywane przez Input System z Unity. |
| `BuildSite` | Poziom zabudowy w scenie: wskaźnik na ekranie → komórki oraz komórki → pozycja w świecie. |
| `BuildingPlacementController` | W każdej klatce: wybiera budynek, znajduje komórki pod wskaźnikiem, pokazuje podgląd, stawia budynek po kliknięciu. |
| `BuildingPlaceholder` | Pudełko zastępujące model budynku: o rozmiarze obrysu i wysokości 8 m. |
| `PlacedBuildingsView` | Nasłuchuje `OnBuildingPlaced` i tworzy pudełko dla każdego postawionego budynku. |
| `IsometricCameraRig` | Kąt kamery, przesuwanie i przybliżanie. |

## Dlaczego takie rozwiązanie

| Możliwość | Dlaczego nie (albo dlaczego tak) |
| --- | --- |
| Swobodne stawianie, nachodzenie sprawdzane fizyką Unity | Liczba możliwych pozycji jest nieograniczona, więc budynki stają prawie na styk albo lekko obrócone; sprawdzenie zależy od kształtów kolizji (colliderów) w scenie i nie da się go testować poza trybem Play. |
| Siatka sześciokątna (jak w *Surviving Mars*) | Dobrze pasuje do okrągłych kopuł, ale obrysy, rozmiary modeli i obliczenia są trudniejsze; nasze budynki są prostokątne. |
| Widoczne linie siatki podczas budowania | Zespół zdecydował, że ich nie będzie: podgląd przyciągany do komórek wystarcza graczowi jako informacja, a widok pozostaje czysty. |
| Budynki dopasowane do wysokości terenu | Każdy model musiałby pasować do każdego zbocza; sąsiednie budynki stałyby na różnych wysokościach. |
| Wyrównywanie terenu pod każdym budynkiem | Wymaga edycji terenu w trakcie gry i zmienia teren przygotowany przez grafika. |
| **Ukryta siatka kwadratowa, jeden poziom zabudowy (wybrane)** | Nachodzenie to sprawdzenie w tabeli, rozmiary to całe komórki, każdy model pasuje w każdym miejscu, a reguły są zwykłym kodem, który można sprawdzić testami. |
| Szukanie wskazanego miejsca fizycznym raycastem w teren | Wymaga kształtu kolizji dla całego terenu (kosztowne przy szczegółowym modelu terenu) i zwraca wysokość terenu, której nie chcemy. Przecięcie promienia z płaską płaszczyzną to jedno krótkie obliczenie. |
| Pakiet kamery Cinemachine | Planowany dla pełnej kamery (T09). Jeszcze niezainstalowany; prototyp używa jednego małego skryptu, więc nie trzeba było dodawać pakietu. |

Dlaczego rozmiar komórki i poziom zabudowy są w zasobie z danymi, a nie w kodzie: to wartości projektowe, które zespół może zmienić (liczby w GDD nie są ostateczne). Ich zmiana to edycja w Inspectorze, a nie zmiana kodu.

## Gdzie to jest w projekcie

- `Assets/_Project/Scripts/Building/`: `GridCell`, `GridArea`, `GroundPoint`, `PlacementResult`, `BuildGrid`, `BuildGridGeometry`, `PlacedBuilding`, `BuildingRegistry`.
- `Assets/_Project/Scripts/Data/`: `BuildGridSettings`, `BuildingDefinition`.
- `Assets/_Project/Data/BuildGridSettings.asset`: wartości siatki.
- `Assets/_Project/Data/Buildings/`: jeden zasób na każdy typ budynku.
- `Assets/_Project/Tests/EditMode/Building/`: testy.
- `Assets/_Project/Scripts/Game/`: `GameBootstrap`, `PrototypeControls`, `BuildSite`, `BuildingPlacementController`, `BuildingPlaceholder`, `PlacedBuildingsView`, `IsometricCameraRig`.
- `Assets/Scenes/Prototype.unity`: scena prototypu (kamera, światło, płaski zastępczy teren o boku 1 km, obiekty `Game Bootstrap`, `Building Placement`, `Placed Buildings`).

## Skąd wiemy, że działa

Testy Edit Mode (**Window > General > Test Runner > EditMode**), łącznie 53:

- `BuildGridGeometryTests`: przyciąganie dla obrysów nieparzystych i parzystych, wskaźnik dokładnie na granicy komórek, wskaźnik poza siatką oraz to, że przyciągnięty środek nigdy nie leży dalej niż pół komórki od wskaźnika.
- `BuildGridTests`: obrys dotykający krawędzi siatki jest dozwolony, a wysunięty o jedną komórkę dalej jest odrzucany; nachodzenie na jedną zajętą komórkę jest odrzucane; stanie tuż obok budynku jest dozwolone.
- `BuildingRegistryTests`: postawienie dodaje budynek i wywołuje zdarzenie jeden raz; drugi budynek na tych samych komórkach jest odrzucany i niczego nie zmienia.
- `GridAreaTests`: obrys o rozmiarze zerowym lub ujemnym jest odrzucany.

Część działająca w scenie (wskaźnik, podgląd, kamera) nie ma testów automatycznych; sprawdza się ją ręcznie:

1. Otwórz `Assets/Scenes/Prototype.unity` i wciśnij Play.
2. Przesuwaj mysz nad terenem: zielone pudełko podąża za nią, skacząc co jedną komórkę.
3. Kliknij lewym przyciskiem: zostaje szare pudełko. Wskaż je ponownie: podgląd robi się czerwony, a kliknięcie nic nie daje.
4. Wciśnij 2, potem 3: podgląd zmienia się na 2 × 2 i 3 × 2 komórki.
5. Przesuwaj kamerę klawiszami W, A, S, D i przybliżaj kółkiem myszy. Przesuń podgląd poza krawędź terenu: robi się czerwony.
6. Zaznacz dowolne postawione pudełko w oknie Hierarchy: jego pozycja Y zawsze wynosi 4 (poziom zabudowy 0 plus połowa 8-metrowej wysokości pudełka), gdziekolwiek stoi.
7. Zatrzymaj grę, zmień `Build Level Height Metres` w `BuildGridSettings.asset` na 5 i wciśnij Play: każde pudełko stoi teraz 5 m wyżej. Przywróć wartość 0.

## Pytania, które mogą paść

**Dlaczego nie widać siatki, skoro gra jej używa?** Siatka to reguła, a nie obraz. Gracz ją wyczuwa, bo podgląd przesuwa się skokami co jedną komórkę. Z rysowania linii zrezygnowaliśmy świadomie: to decyzja projektowa.

**Jak sprawdzacie, że dwa budynki na siebie nie nachodzą?** Każda komórka pamięta, czy jest zajęta. Nowy budynek jest dozwolony tylko wtedy, gdy żadna z jego komórek nie jest zajęta. Nigdy nie porównujemy budynków między sobą.

**Dlaczego wszystkie budynki stoją na tej samej wysokości, skoro teren jest nierówny?** Modele są zbudowane na płytach i palach, które wypełniają odstęp do gruntu. Dzięki jednej wysokości każdy model pasuje w każdym miejscu, a sąsiednie budynki są wyrównane.

**Skąd gra wie, na co wskazuje mysz, skoro pomija teren?** Prowadzi linię od kamery przez wskaźnik i oblicza, gdzie ta linia przecina płaską płaszczyznę na poziomie zabudowy. Dane terenu nie biorą w tym udziału.

**Do czego służy `GameBootstrap`?** To jedyne miejsce, w którym części gry są tworzone i łączone przy starcie sceny. Dzięki niemu żaden skrypt nie potrzebuje obiektu globalnego ani wyszukiwania po nazwie, żeby znaleźć inny skrypt.

**Co dzieje się na krawędzi mapy?** Obrys wystający poza ostatnią kolumnę lub wiersz dostaje odpowiedź „poza siatką” i budynku nie można postawić.

**Dlaczego budynek 2 × 2 jest przyciągany do narożnika, a 1 × 1 do środka komórki?** Środek obrysu parzystego leży na linii między komórkami, a środek nieparzystego w środku komórki. Zawsze wybieramy obrys, którego środek jest najbliżej wskaźnika.

**Dlaczego rozmiar komórki i obrysy nie są wpisane w kodzie?** To wartości balansu. W zasobie z danymi projektant może je zmienić w Inspectorze, a testy nadal działają na własnych, stałych liczbach.

**Czy postawiony budynek można już usunąć, obrócić albo zapisać?** Jeszcze nie. Usuwanie, obracanie, koszty budowy i zapis gry to osobne, późniejsze zadania.
