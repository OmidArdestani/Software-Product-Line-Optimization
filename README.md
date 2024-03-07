# Software Product Line Architecture(PLA) Optimization with NSGA-II Algorithm

## Overview
This project implements a software product line Architecture(PLA) optimization framework using the NSGA-II (Non-dominated Sorting Genetic Algorithm II) algorithm. It aims to optimize various software metrics within a software product line context, such as Cohesion, Coupling, Feature-Scattering, and Granularity.

## Features
- **NSGA-II Algorithm:** Utilizes the NSGA-II algorithm for multi-objective optimization.
- **Metrics Optimization:** Optimizes software architecture and  metrics including PLA-Cohesion, Coupling, and Feature-Scattering.
- **Evaluation:** Evaluates the optimized product line in terms of configurability, reusability, and commonality.

## Inputs and Outputs
- **Input Files:**
    1. **PLA Component Diagram (UML Standard):** Represents the components and their relationships within the software product line.
    2. **PLA Feature Model (SXFM Standard):** Specifies the features and their relationships within the software product line.
    3. **Relationship Matrix (Text File Standard):** Shows the relationships between features and components within the software product line.

- **Output File:**
    - **Optimized PLA Component Diagram (UML Standard):** The optimized component diagram representing the software product line after applying the NSGA-II algorithm.

## Software Production Line Metrics
This repository contains metrics definitions for assessing Software Production Line Architecture (PLA). These metrics help in evaluating the cohesion, commonality, and coupling within a PLA.

### Metrics Definitions
#### 1. PLA-Cohesion
PLA-Cohesion measures the degree of coherence or unity within the architecture by assessing how well each architectural component fulfills multiple features of the feature model, ensuring a cohesive family of software products.

#### 2. Commonality
Commonality controls the balance of shared and unique interfaces within a Software Production Line Architecture (PLA). It ensures that the PLA has an appropriate mix of common (mandatory) and non-common (optional) interfaces to maintain diversity in products while leveraging shared assets.

#### 3. Coupling
Coupling in Software Production Line Architecture (PLA) quantifies the level of interdependence between architectural components. It evaluates the extent to which components rely on interfaces from other components to fulfill their requirements, with lower values indicating a more modular and loosely coupled architecture.


## Requirements
- .NET Framework or .NET Core
- C# IDE (e.g., Visual Studio)
