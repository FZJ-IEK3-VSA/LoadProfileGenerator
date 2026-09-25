<!-- logo:header:start -->
<p align="center">
  <a href="https://www.fz-juelich.de/en/ice/ice-2">
    <picture>
      <source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/FZJ-IEK3-VSA/README_assets/v.1.0.0/ICE2_Logos/JSA-Header-dark.svg">
      <img src="https://raw.githubusercontent.com/FZJ-IEK3-VSA/README_assets/v.1.0.0/ICE2_Logos/JSA-Header.svg" alt="Jülich Systems Analysis" height="80">
    </picture>
  </a>
</p>
<!-- logo:header:end -->

# LoadProfileGenerator

This repository contains the full source code for the LoadProfileGenerator. 

Binaries are available at https://www.loadprofilegenerator.de

The manual is available [here](https://nbn-resolving.org/urn:nbn:de:bsz:ch1-qucosa-209036), in the second part of the author's PhD thesis.

## Contributions

Contributions are highly welcome. Feel free to send me pull requests.

## City Simulation

The CitySimulation project allows simulating a whole city at once. Input scenario configurations can be generated using the [CityScenarioGenerator](https://github.com/FZJ-IEK3-VSA/CityScenarioGenerator).

CitySimulation is parallelized with MPI, so it needs both an MPI implementation and the .NET bindings [MPI.NET](https://github.com/microsoft/MPI.NET). Depending on the system, both have to be installed separately.

### Windows

Install Microsoft MPI, as it is the only implementation supported on Windows.

### Linux

Any MPI implementation can be used, but MPI.NET has to be built by yourself:

- Build MPI.NET following the [installation instructions for Unix](https://github.com/microsoft/MPI.NET#installation-on-unix).
- Copy the resulting `libmpinet.so` and `MPI.dll` into `CitySimulation/lib/` inside the LoadProfileGenerator repository (create the directory if it does not exist).
- Build the project with ```dotnet build CitySimulation -r linux-x64```

### Running a simulation

CitySimulation expects a single argument, the directory containing the scenario to simulate. It can be started on its own:

```
CitySimulation <scenario directory>
```

or as an MPI program, to distribute the simulation over several processes:

```
mpiexec -n 4 CitySimulation <scenario directory>
```

## Plans

- Improve electromobility
- Speed improvements
- International profiles

## License

MIT License

Copyright (c) 2010-2022 Noah Pflugradt (FZJ IEK-3), Peter Stenzel (FZJ IEK-3),  Martin Robinius (FZJ IEK-3), Detlef Stolten (FZJ IEK-3)

You should have received a copy of the MIT License along with this program.  
If not, see <https://opensource.org/licenses/MIT>

## Citation

If you want to use the LoadProfileGenerator for a publication, please cite the following paper:

```
Pflugradt et al., (2022). LoadProfileGenerator: An Agent-Based Behavior Simulation for Generating Residential Load Profiles. Journal of Open Source Software, 7(71), 3574, https://doi.org/10.21105/joss.03574
```

## External Data

The LoadProfileGenerator uses solar radiation profiles from Deutscher Wetterdienst (DWD, www.dwd.de) and from Photovoltaic Geographical Information System (PVGIS, https://ec.europa.eu/jrc/en/pvgis)

## About Us

We are the <a href="https://www.fz-juelich.de/en/ice/ice-2">Institute of Climate and Energy Systems – Jülich Systems Analysis (ICE-2)</a> at the <a href="https://www.fz-juelich.de/en"> Forschungszentrum Jülich</a>.
Our work focuses on independent, interdisciplinary research in energy, bioeconomy, infrastructure, and sustainability. We support a just, greenhouse gas–neutral transformation through open models and policy-relevant science.

# Acknowledgements

### 2010-2016

This software was first developed at

__Technische Universität Chemnitz - Professur Technische Thermodynamik__

### 2016-2020

__Berner Fachhochschule - Labor für Photovoltaik-Systeme__

Part of the Development was funded by the

__Swiss Federal Office of Energy__

## Starting March 2020

Currently development is funded by the Forschungszentrum Jülich - IEK 3.

<a href="https://www.fz-juelich.de/en/iek/iek-3"><img src="https://raw.githubusercontent.com/OfficialCodexplosive/README_Assets/862a93188b61ab4dd0eebde3ab5daad636e129d5/FJZ_IEK-3_logo.svg" alt="FZJ Logo" width="300px"></a>

