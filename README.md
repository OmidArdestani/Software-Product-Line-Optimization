# Software Product Line Architecture Optimization with NSGA-II Algorithm

## Overview
This project implements a software product line optimization framework using the NSGA-II (Non-dominated Sorting Genetic Algorithm II) algorithm. It aims to optimize various software metrics within a software product line context, such as Cohesion, Coupling, Feature-Scattering, and Granularity.

## Features
- **NSGA-II Algorithm:** Utilizes the NSGA-II algorithm for multi-objective optimization.
- **Metrics Optimization:** Optimizes software metrics including Cohesion, Coupling, Feature-Scattering, and Granularity.
- **Evaluation:** Evaluates the optimized product line in terms of configurability, reusability, and commonality.

## Inputs and Outputs
- **Input Files:**
    1. **PLA Component Diagram (UML Standard):** Represents the components and their relationships within the software product line.
    2. **PLA Feature Model (SXFM Standard):** Specifies the features and their relationships within the software product line.
    3. **Relationship Matrix (Text File Standard):** Shows the relationships between features and components within the software product line.

- **Output File:**
    - **Optimized PLA Component Diagram (UML Standard):** The optimized component diagram representing the software product line after applying the NSGA-II algorithm.

## Metrics
### Cohesion
Cohesion measures the degree of relatedness among the elements within a module or component. High cohesion indicates that elements within the module are closely related and work together to perform a single, well-defined task.

### Coupling
Coupling measures the inter-dependencies between modules or components within a system. Low coupling indicates that modules are relatively independent and can be modified or replaced without affecting other parts of the system.

### Feature-Scattering
Feature-Scattering evaluates the distribution of features across the modules or components of a software product line. Low feature-scattering indicates that features are concentrated within specific modules, promoting modularization and reusability.

### Granularity
Granularity assesses the level of detail or abstraction within modules or components. Fine-grained modules have a higher level of detail and are more specialized, while coarse-grained modules are more general-purpose and encompass broader functionality.

## Requirements
- .NET Framework or .NET Core
- C# IDE (e.g., Visual Studio)
