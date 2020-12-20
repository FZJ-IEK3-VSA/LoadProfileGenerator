---
title: 'LoadProfileGenerator: An agent-based behaviour simulation for generating residential load profiles'
tags:
  - energy research
  - load profiles
  - load curves
  - electricity consumption
  - water consumption
  - behaviour simulation
  - electromobility
  - C#
authors:
  - name: Noah Pflugradt
    orcid: 0000-0002-1982-8794
    affiliation: "1, 2"
  - name: Peter Stenzel
    orcid: 0000-0002-3348-0122
    affiliation: "1, 2"
  - name: Leander Kotzur
    orcid: 0000-0001-5856-2311
    affiliation: "1, 2"
  - name: Martin Robinius
    orcid: 0000-0002-5307-3022
    affiliation: "1, 2"
  - name: Detlef Stolten
    orcid: 0000-0002-1671-3262
    affiliation: "1, 2, 3"
affiliations:
 - name: Jülich Aachen Research Alliance, JARA-Energy, Jülich, Aachen, Germany
   index: 1
 - name: Institute of Techno-economic Systems Analysis (IEK-3), Forschungszentrum Jülich GmbH, Wilhelm-Johnen-Str., D-52428 Jülich, Germany
   index: 2
 - name: Chair for Fuel Cells, RWTH Aachen University, c/o Institute of Techno-economic Systems Analysis (IEK-3), Forschungszentrum Jülich GmbH, Wilhelm-Johnen-Str., D-52428 Jülich, Germany
   index: 3
date: 27 November 2020
bibliography: references.bib

---

# Summary

In the field of energy research, a lot of research questions need residental load profiles as input data. A load profile describes 
for every step in time how much energy was consumed. 

There are two major approaches for generating load profiles: The stochastic approach the most simple form works with rules like 
"70% chance of using the toaster for breakfast on Sundays between 8:00 and 10:00". To achieve realistic profiles of course much more details are needed, 
such as occupancy modelling, dependencies between activities, combination of devices (coffee machine and toaster or washer and dryer for example) and many more.
The stochastic approach is well-suited for situations where a lot of data is available for generating the probability distributions or where large numbers of 
load profiles are needed and the accurate profile of the individual household is not critical.
The second approach is building a detailed behavior simulation and use that to generate the load profile, since a 
large part of the residential energy consumption directly depends on the actions of the residents. This approach is much more complex, 
but yields more realistic profiles for individual households. A detailed review of different approaches of synthesizign load profiles can be found for example in [xxx grandjean]

The LoadProfileGenerator (LPG) implments the second approach: It implements a desire-driven human behaviour model to simulate when people are doing what and where they are. It was originally developed as PhD-thesis [@pflugradt_modellierung_2016].
The human actions are then used to calculate energy consumption and generate the load profiles. 
This is combined with a lot of additional details for more realistic behaviour modelling, such as illnesses, vacations, joint activities between multiple residents such as eating dinner together, 
varying shifts from one day or one week to the next, holidays, a detailed per-room lighting model, automatically adjusted dishwasher and washing machine frequency and much more.

The LPG also contains a full mobility model that tracks for every person where they are and what transport they use to get to other locations, 
which makes it possible to also generate for example charging profiles for electric cars, electric bicycles and other vehicles. The different load profiles have a very strong influence on the self consumption
for a photovolatic system for example, as has been shown in [@pflugradt_impact_2019]

Automatically switching out the devices in a household is a non-trivial task, since multiple activities might use the same device with different 
load profiles, for example a washing machine might be used to wash 30°C laundry and 60°C laundry.
To solve this problem the LPG has an abstraction layer built into the data model that makes sure the correct device load profiles are used.

For the simulation of larger neighborhoods, villages or cities it is not possible to simply scale up a single load profile, since that leads to very 
unrealistic load spikes, since in a scaled up load profile 
essentially everyone is modelled as performing their activities at exactly the same time in parallel, which does not reflect reality.
To enable the simulation of large numbers of households the LPG includes a full rule-based templating engine. With this it is possible 
to specify households with essentially "a household with a working couple,
a husband that works in an office 9am to 5pm and a wife that works from about 7am to 3pm and each have two hobbies". The LPG can 
then generate from this description an unlimited number of versions of the household that vary 
for example in the hobbies, in the cooking behaviour, hygiene habits and more. This solves the issue of the load spikes by 
simulating a realistic level of concurrency. That this works has been shown in previous publications [@pflugradt_synthesizing_2019], [@pflugradt_simzukunft_2018].

The LPG offers both a Windows GUI and a command line interface, which can be automated for example with Python. The full code base is 
about 60.000 lines of code and includes large amount of unit and integration tests. The LPG comes with 60 predefined households for Germany, 
but is fully customizable and can be adapted to any other country.

In conclusion the LPG is a tool to synthesize highly detailed load profiles for water, electricity, behaviour, electric vehicle usage, electric vehicle charging, occupancy and more. 

# Novelty of this publication

The LPG has been in development since 2010, has been available free of charge the entire time, but it has only recently been open sourced 
in Febrary 2020 under the MIT-License and this is the first publication describing the LPG as an open-source tool. Contributions and merge requests are greatly appreciated.

# Statement of need

Load profiles are used for example to calculate the self consumption of a residential photovoltaic system, the best way to run a battery, to evaluate different algorithms in a SmartGrid-simulation and many more applications.

Load profiles are needed either for the present situation or the future.

For the present, it would be preferable to use measured load profiles. But measured residential load profiles are very sensitive from a privacy standpoint, since it is possible to draw a 
lot of conclusions about the residents from residential load profiles. Therefore no large data sets of public residential load profiles measured in a high resolution exist.
This leaves load profile synthesis is the only option for most applications.

For the future by defintiion no measured load profiles can exist, so for calculating future grids, load profile synthesis is mandatory.

The `LPG` aims to solve these problems by providing a free and easy to use tool that enables researchers and planners to generate custom profiles very quickly.

There are a number of different tools for load profile synthesis available, but the LPG introduced in this paper is to the best knowledge of the authors the most detailed and the only one doing a full desire-driven behaviour simulation.
Additionally it is the only one that is covering the full spectrum of energy consumption (electricity, water, mobility, heat).

It is planned to continue to develop and extend the LPG. For example, the mobility model could be extended with more realistic preferences, 
the house infrastructure modelling could be improved, and the device database could be extended.


# Acknowledgements

From 2010 to 2013 the development was supported by the Technische Universität Chemnitz, Professur Technische Thermodynamik in Germany.
From 2016 to 2020 the development was supported by the Berner Fachhochschule, Laboratory for Photovoltaik Systems in the research project SimZukunft in Switzerland.
Starting form March 2020 the development has been supported by the Forschungszentrum Jülich - IEK 3.


# References

The LPG has been used in many paper for various applications. To demonstrate what the LPG can be used for, six citing papers have been randomly selected:

- [@harder_cost_2020] used the LPG to analyze operating flexibility in electricity grids. 
- [@bockl_hyflowhybrid_2019] used the generated profiles for an evaluation of sector coupling.
- [@lovati_optimal_2020] used the model to evaluate peer-to-peer electricity trading while considering the influence of lifestyle.
- [@boeckl_sizing_2019] evaluated the optimal size of photovoltaik systems for different load profiles. 
- And building on that [@lovati_design_2020] evaluated the impact of the load profile on optimal photovoltaik system operation.
- [@haider_data_2020] analyzed optimal ways of charging selectric cars.
  
  




