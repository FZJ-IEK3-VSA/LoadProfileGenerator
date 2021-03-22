---
title: 'LoadProfileGenerator: An Agent-Based Behavior Simulation for Generating Residential Load Profiles'
tags:
  - energy research
  - load profiles
  - load curves
  - electricity consumption
  - water consumption
  - behavior simulation
  - electro mobility
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

A load profile contains a value for each step in time that describes how much energy was consumed in that time step. There are load profiles for example for electricity, gas, heat demand and other load types.
This is critical input data for a lot of research applications. For example, the car charging load profiles have a very strong influence on the self-consumption
for a photovoltaic system, as has been shown in [@pflugradt_impact_2019]. Other examples for applications are given in the section below. 

For most applications it would be ideal to have access to measured data. But such data is frequently not available. Reasons for that include
privacy concerns, companies treating all measurements as trade secrets, lack of instrumentation or, especially in research about future energy systems, 
that  the situation simply doesn't exist yet and thus can't be measured. Especially in the residential area privacy is a major concern since is easy to 
draw detailed conclusions about the life of the residents from the load profiles. If no measured data is available, then the creation of synthetic profiles is the only solution.  

This paper describes the LoadProfileGenerator (LPG), an application that focuses on creating synthetic residential load profiles. It uses a behavior-driven agent simulation 
to model the behavior of the residents in detail and generate load profiles for the residential energy consumption, primarily electricity and domestic hot water. 
Additionally, it generates behavior data, travel data and location data. 

The LPG is focuses on modelling single households as accurately as possible instead of stochastic approaches that focus more on getting the city or the district approximately correct. The LPG comes with 60 predefined households.
The households in the LPG are fully customizable and can be specified by specifying people and their traits. Examples for traits are "alarm at 7am", "sleeps 8h" or "works at the office from 9-5". There are over 400 traits predefined and more can be added by the user. To enable the modelling of a large number of households, the LPG contains functions to automatically create new household definitions from the traits, which is described in more detail below.

The LPG does not currently contain a detailed heating energy simulation, since that field is already covered in detail by other tools such as TRNSYS, Polysun or EnergyPlus.


# Statement of need

Load profiles are used by researchers, planners and others. They are used for modelling building energy systems, interactions with the electricity, gas or district heating grids, 
when trying to develop new technologies such as energy management systems or smart grid control algorithm and in many other applications.
When modelling building energy systems for example, the self-consumption of the PV energy depends strongly on the load profile. When modelling a grid, the load at any point in time depends on the simultaneity between the load profiles and 
when developing smart grid algorithms for energy trading between prosumers, the trading potential depends in the difference between the profiles of each household.
Load profiles are needed either for the present situation or the future, depending on the application.

For the present, it would be preferable to use measured load profiles. But measured residential load profiles are very sensitive from a privacy standpoint, since it is possible to draw a 
lot of conclusions about the residents from residential load profiles. Therefore, no large public data sets of residential load profiles measured in a high resolution exist.
This leaves load profile synthesis is the only option for most applications.

For the future no measured load profiles can exist, so for calculating future grids, load profile synthesis is the only option.

The `LPG` aims to solve these problems by providing a free and easy to use tool that enables researchers and planners to generate custom profiles very quickly.

There are a number of different tools for load profile synthesis available, such as CREST [xxx] or Synpro [xxx], but the LPG introduced in this paper is to the best 
knowledge of the authors the most detailed and the only one doing a full desire-driven behavior simulation.

![Difference between the average over many households and measurements from a single household. The high peaks are from cooking. [@pflugradt_modellierung_2016] .\label{fig:hzero}]](h0vsmeasurement.png)


## Examples for Previous Usage

The LPG has been used in many papers for various applications. To demonstrate what the LPG can be used for, six citing papers have been selected that give a good overview of the possible applications:

- [@harder_cost_2020] used the LPG to analyze operating flexibility in electricity grids. 
- [@bockl_hyflowhybrid_2019] used the generated profiles for an evaluation of sector coupling.
- [@lovati_optimal_2020] used the model to evaluate peer-to-peer electricity trading while considering the influence of lifestyle.
- [@boeckl_sizing_2019] evaluated the optimal size of photovoltaic systems for different load profiles. 
- And building on that [@lovati_design_2020] evaluated the impact of the load profile on optimal photovoltaic system operation.
- [@haider_data_2020] analyzed optimal ways of charging electric cars.
  

# Background

There are two major approaches for generating residential load profiles: Stochastic or agent-based. The different approaches are explained in detail in for example Grandjean et al. [@grandjean_review_2012] [xxx] [xxx, xxx, swan]
The stochastic approach in the most simple form works with probability rules for activities or device activation such as 
"70% chance of using the toaster for breakfast on Sundays between 8:00 and 10:00". To achieve realistic load profiles much more details are needed, 
such as occupancy modelling, dependencies between activities, combination of devices (coffee machine and toaster or washer and dryer for example) and many more.
While it is entirely feasible to generate realistic probability distributions across larger populations, the stochastic approach is not ideal for modelling individual households.
The biggest hurdle is that people's behavior tends to vary across different days and different seasons. So the more realistic and detailed a profile needs to be, the more detailed and finely-grained probability distributions are needed. For example modelling a person that visits the fitness studio every Tuesday requires a special probability distribution for Tuesdays. Modelling a person that spends a lot of time in
the garden in the summer requires a special probability distribution for summers. And someone that visits fitness studios on Tuesdays and spends their afternoons in the 
garden in the summer needs probability distributions for winter Tuesdays, summer Tuesdays, winter weekdays, summer weekdays and so on. It is obvious that the requirements escalate rapidly when trying to model realistic individual households.
So instead it is accepted in stochastic models that the individual household might not be perfect and the probability distributions are tuned until the average over the entire population is satisfactory.
Another problem is that with simple probability distributions nothing prevents people from taking two showers in a row or performing other rather unrealistic behavior patterns.
The stochastic approach is well-suited for situations where a large data set is available for generating the probability distributions or where large numbers of 
load profiles are needed and the accurate profile of the individual household is not critical.

The second approach is building a detailed behavior simulation and use that to generate the load profile, since a 
large part of the residential energy consumption directly depends on the actions of the residents. This approach is much more complex, 
but can yield more realistic profiles for individual households, because the agent model inherently 
includes things like occupancy modelling and makes it easy to implement for example activity tracking to make sure people don't shower twice in a row. 

# Method

The LoadProfileGenerator (LPG) implements the second approach: It implements a desire-driven human behavior model to simulate when people are doing what and where they are. 
The basic algorithm for selecting an activity is shown in \autoref{fig:happiness}.

![Basic idea behind the activity selection process.\label{fig:happiness}]](lpg_happiness.png)

It was originally developed as PhD-thesis [@pflugradt_modellierung_2016] and has since then been extended and improved. 

The behavior of the residents is used to calculate energy consumption and generate the load profiles by combining synthetic and measured device profiles. 
This is combined with additional details for more realistic behavior modelling, such as illnesses, vacations, joint activities between multiple residents such as eating dinner together, 
varying shifts from one day or one week to the next, holidays, a detailed per-room lighting model, automatically adjusted dishwasher and washing machine frequency and much more.
The method for selecting an activity is shown in \autoref{fig:execution}.

![Simplified flow chart of the activity execution process and generation of load profiles.\label{fig:execution}]](activityexecution.png.png)

The LPG is written in C# and has about 60.000 lines of code. 

# Features

The LPG has the following features for generating better load profiles:

**Templating Engine**
To enable the simulation of large numbers of households the LPG includes a full rule-based templating engine. With this it is possible 
to specify households abstract households templates. The LPG can then generate an unlimited number of versions of the household that vary 
for example in the hobbies, in the cooking behavior, hygiene habits and more. Scaling up a load profile from a single household essentially models everyone as performing their activities 
at exactly the same time in parallel, which does not reflect reality. By using the templating function the LPG simulates a realistic level of concurrency, 
as shown in previous publications [@pflugradt_synthesizing_2019], [@pflugradt_simzukunft_2018].

**Mobility Model** The LPG contains a full mobility model that tracks for every person where they are and what transport they use to get to other locations, 
which makes it possible to also generate for example charging profiles for electric cars, electric bicycles and other vehicles. 

**GUI** The LPG offers a Windows GUI that enables new users to easily modify existing household definitions or add new ones.

**Command Line Interface** To enable users to do mass simulations, automate and to integrate with other software the LPG comes with a command line interface.
This allows the user to specify the calculation as a JSON file and feed the input to the simulation engine.

**Modelling Behavior Change**
This has a major advantage over the stochastic approaches in other load profile generators such as CREST [xxx] or SynPro [xxx]: With the LPG it is  
easy to model both the impact of homogenous resident such as neighborhoods with lots of young families, a retirement community or a student neighborhood and to analyze the impact of changing 
behavior patterns or population structures.

**Fully Customizable: ** While the predefined households are only validated for Germany, the LPG is fully customizable and can be adapted to any country. For this it would just be necessary to add country-specific devices and behavior patterns and combine them into typical households for the country. For example for Spain one big adjustment would be to shift the typical dinner time from the German 17:00-19:00 to the Spanish 21:00-23:00.

**Device Switching:** Automatically switching out the devices in a household is a non-trivial task, since multiple activities might use the same device with different 
load profiles, for example a washing machine might be used to wash 30°C laundry and 60°C laundry. To solve this problem the LPG has an abstraction 
layer built into the data model that makes sure the correct device load profiles are used.

**Well-tested:** The project has been in production use since 2011 and includes almost 500 unit and integration tests for automated testing.

**MIT-License:** The LPG can be used in other projects without license issues.


# Novelty of this publication

The LPG has been in development since 2010, has been available free of charge the entire time at http://www.loadprofilegenerator.de, but it has been open sourced only
in February 2020 under the MIT-License. This is the first publication describing the LPG as an open-source tool. 
Contributions and merge requests are greatly appreciated.


# Acknowledgements

From 2010 to 2013 the development was supported by the Technische Universität Chemnitz, Professur Technische Thermodynamik in Germany.
From 2016 to 2020 the development was supported by the Berner Fachhochschule, Laboratory for Photovoltaik Systems in the research project SimZukunft in Switzerland.
Starting form March 2020 the development has been supported by the Forschungszentrum Jülich - IEK 3.




