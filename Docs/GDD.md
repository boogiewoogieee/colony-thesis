## **Game Concept Document — Bachelor's Thesis (v2)**

*Changes from v1: grid-based building, upgrades capped at level 2, tutorial flow, dome activation as the trigger for raids, global storage (including energy), rain, food and water upkeep, the new resource "Tools", turrets, colonist summoning, a loss condition, timer-driven grass growth, and game saving. Items still to be decided are marked **TBD**.*

### **1. Project Description**

A sci-fi economic strategy game in which the player manages a colony on a hostile planet. The goal is to develop infrastructure, manage resources and colonists, expand the protective dome, and gradually restore life to the barren terrain.

The colony must deal with limited resources, changing weather conditions, and monster attacks. The player makes decisions regarding base expansion, production, expeditions, and defense, striving to keep the colony fed, supplied and operational.

The project is being developed by a four-person team as part of a bachelor's thesis.

### **2. Core Assumptions**

* **Genre:** Economic strategy with elements of base building, exploration, and defense.
* **Camera:** Isometric.
* **World:** A square map of limited size.
* **Building:** Grid-based placement of structures with predefined sizes (similar to *Cities: Skylines*).
* **Main Goal:** Developing the colony and gradually greening the planet.
* **Main Challenges:** Feeding and supplying colonists, managing energy, production and tools, as well as handling expeditions, weather, and monster attacks.
* **Saving:** The game can be saved and loaded.

The game is intended to maintain a limited scope appropriate for implementation within a bachelor's thesis. Exploration of further areas takes place through an expedition system, without the need to create a vast open world or additional locations.

### **3. Map and Building System**

The game world consists of a square map depicting a fragment of a hostile planet. The colony is built in the central part, which will later be protected by an electromagnetic dome.

Buildings are placed on a grid. Each structure has a predefined size in grid cells and a fixed collision footprint.

Basic rules:

* Buildings occupy whole grid cells and cannot overlap with each other or with obstacles.
* Mining drills can only be placed on cells covering an appropriate resource deposit.
* Most of the colony's infrastructure is located inside the dome.
* Resource deposits spawn randomly on the map, primarily outside the initial protected area, so drills are often built outside the dome.
* Structures outside the dome are exposed to monster attacks (see Section 13).

### **4. Tutorial, Protective Dome and Command Center**

**Tutorial.** The game starts without a dome. The tutorial guides the player through placing all essential buildings. Completing the construction of the dome is the final step of the tutorial.

**Dome activation.** Activating the dome draws the attention of the planet's hostile creatures toward the colony. From this moment on, raids begin and the player must start planning base defenses.

**Command Center.** The Command Center is the main building of the colony. It generates the electromagnetic dome and communicates with the remaining infrastructure.

The dome protects the colony from hostile environmental conditions and defines the primary area for the safe operation of the base. Upgrading the Command Center to level 2 increases the dome's range, enabling colony expansion and granting access to further areas of the map.

The Command Center is also a strategic target during monster attacks. Its destruction causes communication disruption, the permanent loss of the current expedition, and a temporary blockade on summoning new colonists.

### **5. Colony Buildings and Modules**

15 types of structures are planned for the game.

**1. Residential Building**
Increases the maximum number of inhabitants that can live in the colony. Level 2 provides greater capacity.

**2. Bunker**
Provides shelter for inhabitants during monster raids and severe weather conditions.

**3. Command Center**
Generates the protective dome and is responsible for colony communication. Level 2 increases the dome's range. It is one of the most important objects to protect during raids.

**4. Factory**
Processes mined materials into components, construction materials and Tools. Production takes place in a queue; the factory's level determines how many items are produced simultaneously.

**5. Expedition Hangar**
Enables organizing and sending expeditions beyond the visible map area. The player selects the destination and the number of colonists taking part.

**6. Laboratory**
Allows unlocking new technologies and upgrades. Research can unlock more advanced buildings, mining devices, and production recipes. Research cost: **TBD**.

**7. Landing Pad**
Receives new colonists summoned from the mothership. Each summon costs a fee (**TBD**: which resource or currency, and how much). In the event of communication disruption, it is temporarily taken out of service.

**8. Rainwater Treatment Plant**
Collects and filters rainwater during rain. The water is used for colonists' survival and in greenhouse production.

**9. Solar Panels**
Harvest energy from sunlight. They produce energy only during the day and depend on light availability.

**10. Wind Turbines**
Generate energy from air movement. They operate only when wind is present, complementing the solar panels.

**11. Silo**
Each silo increases the global storage capacity for items by a fixed amount.

**12. Greenhouse**
Produces food. Consumes water. (**TBD**: whether it also produces plants for terraforming, now that grass growth is driven by a timer, see Section 11.)

**13. Mining Drill**
Extracts resources from deposits found on the map. It can only be placed on an appropriate deposit. The type of extracted material depends on the deposit.

**14. Military Outpost**
Enables patrolling and defending the territory within its area of operation. (**TBD**: how its role differs from the turret's.)

**15. Turret**
Defends structures, including those located outside the dome, against hostile creatures.

**Upgrades.** Selected buildings can be upgraded, up to level 2 only. Upgrades require specific materials and energy and increase efficiency, capacity, or effectiveness. An upgrade may also replace the current model with a more advanced visual variant.

### **6. Colonists and Population Management**

Colonists constitute one of the primary resources of the colony.

* New colonists are summoned from the mothership for a fee and arrive via the landing pad.
* Each operating module requires assigning an appropriate number of colonists.
* More advanced buildings may require a larger crew.
* Residential buildings increase the maximum number of inhabitants.
* Colonists sent on an expedition are temporarily unavailable for work in the base.
* The bunker allows sheltering inhabitants during raids and hazardous weather conditions.

**Food and water.** Colonists need food and water to survive. Every day, the colony's stored food and water determine how many colonists can be supported (the "food budget"). Colonists who do not fit within the budget die, one day at a time. Daily consumption per colonist: **TBD**.

### **7. Global Storage**

All resources are stored in one global pool; there is no transport between buildings.

* The global storage holds items (raw materials, components, Tools), food, water and energy.
* Each silo increases item storage capacity by a fixed amount.
* **TBD:** the base capacity, how food, water and energy capacity grow, and what happens to production when storage is full.

Because energy is stored, the colony can keep the dome running at night and during calm weather, as long as reserves last.

### **8. Resources and Deposits**

Material deposits appear randomly on the map, primarily outside the initial area protected by the dome. Each deposit provides access to a specific material and requires a drill placed on it.

**Coal Deposit** — used for producing steel and other materials requiring this resource.

**Iron Deposit** — a basic material used for producing steel and in simpler construction recipes.

**Copper Deposit** — used for producing electronics and in less complex recipes.

**Silicon Deposit** — essential for producing more advanced electronics and complex components.

**Oil Deposit** — a raw material used for producing plastics.

**Ice Deposit** — a source of drinking water for the colony.

**Tools (manufactured).** Tools are produced in the factory. Keeping an adequate supply of Tools in storage prevents buildings from deteriorating. **TBD:** the Tools recipe, consumption rate per building, and what a deteriorated building loses (efficiency, or it stops working).

### **9. Production and Crafting**

Basic flow:

1. Drills extract materials from available deposits.
2. Resources go into global storage.
3. The factory processes them into construction materials, components and Tools.
4. Manufactured materials are used to build, upgrade and maintain infrastructure.
5. The laboratory allows unlocking new technologies and more advanced recipes.

Example dependencies:

* Coal and iron are used to produce steel.
* Copper is used to manufacture basic electronic components.
* Silicon is needed for more advanced electronics.
* Oil serves as a raw material for producing plastics.
* Ice and rain provide water.

#### **Production Queue System**

The player selects recipes, specifies the number of items to manufacture, and adds orders to the production queue. Each item requires resources and a specific production time.

* **Level 1:** one item produced at a time.
* **Level 2:** two items produced simultaneously.

Upgrading the factory increases throughput but does not necessarily shorten the time of a single item. Exact recipes, quantities and times will be determined during implementation.

### **10. Energy and Weather Conditions**

Energy is essential for operating infrastructure and maintaining the dome. Surplus energy is kept in global storage.

**Solar Panels** produce energy only during the day. **Wind Turbines** produce energy only when wind is present.

Weather events:

**Rain**
Enables the Rainwater Treatment Plant to collect water. Rain frequency and duration: **TBD**.

**Solar Eclipse**
Temporarily reduces or completely halts energy production by solar panels.

**Sandstorm**
Poses a threat to the colony and can hinder the functioning of infrastructure. Inhabitants can use the bunker to reduce the risk.

### **11. Terraforming and Vegetation Growth**

Grass models are present in the game world from the start, at size 0. Grass grows only in areas located under the dome.

* A 4-hour timer governs the state of the grass.
* Grass grows by 20% of its full size each in-game day, so it reaches full size after 5 days. **To confirm:** this implies an in-game day of 48 minutes of play time.
* The timer counts play time, pauses with the game, and is stored in save files.
* **TBD:** when the dome expands, whether newly covered grass starts from 0 at that moment.

The greening process is a visible indicator of colonization progress. Technically, each grass area scales its models according to its growth value, and the terrain material can blend soil toward grass as growth increases.

### **12. Expeditions**

Expedition flow:

1. The player selects the expedition destination.
2. Specifies the number of colonists sent.
3. The expedition lasts for a set amount of time.
4. Upon completion, the player receives a report along with acquired resources or items.

A larger expedition may increase the chances of success but reduces the number of workers in the colony. Expeditions provide access to rare resources and items not available through mining. The destruction of the Command Center during a raid causes the permanent loss of the current expedition.

### **13. Defense System and Monster Attacks**

Raids begin once the dome has been activated (end of the tutorial).

**Attacks on the dome:**

1. A monster attack on the colony begins.
2. Enemies attack the protective dome.
3. The generator consumes stored energy to maintain and regenerate the barrier.
4. Once energy is depleted, the attacked section of the dome temporarily disappears.
5. Monsters breach the colony and can be engaged by defensive infrastructure.
6. If the enemies do not achieve their goal within a specified time, they retreat.

**Attacks outside the dome:** hostile creatures can attack structures located outside the dome, such as drills. Turrets defend them.

**Collisions and tracking:** buildings have fixed collision footprints; enemies use a simple tracking system to find and follow their targets.

**Consequences of a lost raid** (Command Center reached):

* The current expedition is permanently lost.
* The landing pad is temporarily blocked and cannot receive new colonists.
* Colonist morale drops, causing a 20% reduction in the work efficiency of the entire colony.

The morale penalty lasts for a specified duration. Subsequent failed raids refresh its duration but do not stack.

### **14. Core Gameplay Loop**

1. **Resource Extraction:** drills extract materials from deposits.
2. **Energy Production:** solar panels and wind turbines produce energy; surplus goes to storage.
3. **Survival:** food and water keep colonists alive.
4. **Processing:** the factory produces materials, components and Tools.
5. **Maintenance:** Tools in storage prevent buildings from deteriorating.
6. **Infrastructure Development:** the player builds and upgrades modules.
7. **Population Management:** colonists are summoned for a fee and assigned to modules.
8. **Expansion:** the Command Center upgrade increases the dome's range.
9. **Exploration:** expeditions supply rare resources and special items.
10. **Terraforming:** grass grows under the dome over time.
11. **Defense:** turrets, outposts and the dome protect the colony.

### **15. Victory and Loss Conditions**

**Victory** (proposed): reaching a specific level of colony development, for example the Command Center at level 2, a specified portion of the dome area fully greened, required modules built, key technologies unlocked, and selected rare materials acquired. Final requirements: **TBD**.

**Loss:** a lack of food and drink. Each day, colonists outside the food budget die. **TBD:** whether the game ends when no colonists remain, or at a threshold.

### **16. Saving**

The game state can be saved and loaded: buildings and their levels, storage contents, colonists and assignments, the production queue, research, the dome, grass growth, weather, active expeditions and raid state. **TBD:** manual saves, autosave, or both.

### **17. Project Scope Assumptions**

* The map remains limited in size.
* Expeditions take place off-map and do not require additional locations.
* Building is grid-based, with predefined building sizes.
* Upgrades go up to level 2 only.
* Resources are stored globally, without logistics.
* Combat uses simplified defensive mechanics and simple enemy tracking.
* Production and crafting rely on specific recipes and a production queue.
* Colonist management is based on the number of workers and their assignment to modules.
* Terraforming uses timer-driven growth of pre-placed grass models.
* Weather events affect the functioning of selected infrastructure elements.
