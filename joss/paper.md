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
 - name: Chair for Fuel Cells, RWTh Aachen University, c/o Institute of Techno-economic Systems Analysis (IEK-3), Forschungszentrum Jülich GmbH, Wilhelm-Johnen-Str., D-52428 Jülich, Germany
   index: 3
date: 04 April 2021
bibliography: references.bib

---

# Summary

A load profile contains a value for each step in time that describes how much energy was consumed in that particular time step. There are load profiles for example for electricity, gas, heat demand and other load types.
This is critical input data for both energy system planners and researchers. For instance, car charging load profiles have a very strong influence on the self-consumption of a photovoltaic system. [@pflugradt_impact_2019]. 

This paper describes the LoadProfileGenerator (LPG), an application for creating synthetic residential load profiles. It uses a desire-driven agent simulation 
to model the behavior of the residents in detail and generate load profiles with the high temporal resolution of 1 minute for residential energy consumption, primarily electricity and domestic hot water. 
Additionally, it generates behavioral data, travel and locational data. 

The LPG features 60 predefined households, validated for Germany. These households  are fully customizable and can be defined 
by specifying the people residing in them and their traits. Examples of traits are "alarm at 7am", "sleeps 8h" or "works at an office from 9-5". 
There are over 400 predefined traits and more can be added by users. To enable the modeling of a large number of households, 
the LPG contains functions for automatically creating new household definitions on the basis of traits. 
The LPG is written in C# and comprises about 60.000 lines of code. Unit test code coverage is greater than 70%. 

The LPG does not currently contain a detailed heating demand simulation, as that field is already covered in detail by other tools such as TRNSYS, Polysun, tsib [@kotzur_bottom-up_2020] or EnergyPlus.


# Statement of need

Load profiles are utilized by researchers, planners and others to model energy systems in buildings, interactions with electricity, gas or district heating grids, 
when developing new technologies such as energy management systems or smart grid control algorithms and in many other applications.

Averaged profiles over a larger group of individual households are not an valid option for applications such as system simulations and analysis with a high spatial resolutions, since they have a significantly different shape than real individual load profiles, as shown in \autoref{fig:hzero}.

![Difference between the average load curve over many households and measurements from a single household. The high peaks in the individual household are produced by cooking. [@pflugradt_modellierung_2016] .\label{fig:hzero}](h0vsmeasurement.png)

For most applications it would be ideal to have access to measured data.  However, such data is frequently unavailable. The reasons for this include
privacy concerns, data availability, lack of instrumentation or, especially in research on future energy systems  
that the situation simply does not exist and therefore cannot be measured. Especially in residential sector privacy is a major concern since is easy to 
draw detailed conclusions about the life of residents on the basis of the load profiles. If no measured data is available, then the creation of synthetic profiles is the only solution.  


The `LPG` seeks to solve these problems by providing a free and easy to use tool that enables researchers and planners to generate custom profiles very quickly.

There are numerous different tools for load profile synthesis available, such as CREST [@mckenna_high-resolution_2016] and SynPro [@fischer_stochastic_2016], but the LPG  is to the best knowledge of the authors the most detailed and the only one that makes use of a fully desire-driven behavioral simulation based on a psychological model.



## Examples for Previous Usage

The LPG has been utilized in many studies for various applications, such as the following:

- @harder_cost_2020 analyzed operating flexibility in electricity grids. 
- @bockl_hyflowhybrid_2019 generated load profiles for an evaluation of sector coupling.
- @lovati_optimal_2020 used the model to evaluate peer-to-peer electricity trading while considering the influence of lifestyle.
- @boeckl_sizing_2019 evaluated the optimal size of photovoltaic systems for different load profiles. 
- Building on that, @lovati_design_2020 evaluated the impact of load profiles on optimal photovoltaic system operation.
- @haider_data_2020 analyzed the optimal way of charging electric cars.
  
# Methodology

The LPG implements a desire-driven model of human behavior to simulate the whereabouts and activities of residents. 
The basic algorithm for selecting an activity is shown in \autoref{fig:happiness}.

![Basic overview of the activity selection process.\label{fig:happiness}](lpg_happiness.png)

The behavior of the residents is used to calculate power consumption and generate the load profiles by combining both synthetic and measured device profiles. 
This is further combined with additional details to enable more realistic behavior modeling, such as illnesses, vacations and joint activities between multiple residents such as eating dinner together, or going shopping.

# Novelty of this Study

The LPG has been in development since 2010, originally in the context of a PhD-thesis [@pflugradt_modellierung_2016]. Since then it has been further extended and improved upon. An open access graphical user interface based version was since LPG's inception available free of charge at http://www.loadprofilegenerator.de. In February 2020 the LPG was published open source under the MIT-License. This is the first publication that describs the LPG as an open-source tool, which is now available at https://github.com/FZJ-IEK3-VSA/LoadProfileGenerator.  Further Contributions and merge requests are greatly appreciated. Additionally, there is now an open-source Python wrapper around the LPG both for Windows and Linux to enable easier automation. The Python-Wrapper is located at https://github.com/FZJ-IEK3-VSA/pylpg.

# Acknowledgements

From 2010 to 2013 development was supported by the Technische Universität Chemnitz, Professur Technische Thermodynamik in Germany.
From 2016 to 2020 development was supported by the Berner Fachhochschule, Laboratory for Photovoltaik Systems in the SimZukunft research project in Switzerland.
Starting from March 2020 development has been supported by the Forschungszentrum Jülich GmbH - Institute for Energy and Climate Research, Techno-Econonomic Systems Analysis (IEK-3).

# References
