# Empathetic Agent with Keyword-Gated Expression Mapping for AR/VR Environments

## Abstract

This project, developed for my MSc dissertation, presents a novel implementation of real-time emotional responses in a virtual agent. Building upon existing principles of human-computer interaction, I have developed a **Keyword-Gated Expression Mapping** model to address the challenge of creating believable, context-aware reactions in immersive **AR/VR environments**. The system analyzes pre-scripted speech to identify key emotional triggers, translating them into quick, realistic facial micro-expressions. The model's design prioritizes low computational overhead and flexibility, making it ideal for resource-constrained platforms like the **Oculus Meta Quest 3**.

---

## Research and Technical Context

### Research Hypothesis

The central hypothesis of this work is that a **rule-based emotional model**, driven by keyword-gated expression mapping, can produce perceptually convincing and emotionally resonant facial reactions in a virtual agent with minimal computational cost. This approach aims to validate a method for creating reactive virtual characters that enhance user immersion and engagement in immersive computing contexts.

---

### Empathy Model: Keyword-Gated Expression Mapping

The core of this project is a rule-based system for activating specific facial expressions based on emotional cues.

- **Core Logic**: The model processes transcribed speech, using pre-defined keywords as triggers for specific emotions.
  
- **Activation**: When a keyword is detected and an arousal threshold is met, the corresponding emotion is triggered, resulting in a quick facial reaction. This allows for moment-based expressiveness and supports layered micro-expressions.
  
- **Timecourse**: Reactions have a fixed decay duration (e.g., 1.5 seconds) to ensure they are believable and do not override the agent's base emotional state. An optional cool-down window is included to prevent "flickering" between expressions.

---

### Design Considerations & Future Work

The model is designed to be **foundational** and **extensible**. A potential enhancement for future iterations is to incorporate a **prosody (tone) analysis** to provide a more nuanced emotional activation. This could be mapped to models like **Russell's Circumplex** (valence/arousal).

---

## Technical Stack

- **Platform**: Unity
- **Hardware**: Oculus Meta Quest 3/2
- **Emotional Model**: The system is designed to work with Ekman's 6 basic emotions:
  - **Joy**
  - **Sadness**
  - **Anger**
  - **Fear**
  - **Surprise**
  - **Disgust**

---

## Setup and Usage

### Prerequisites

Before running the project, ensure you have the following:

- **Unity Hub** with Unity 2021.3.17f1 or later.
- **Visual Studio Code** with the C# and Unity extensions for scripting.
- **Oculus Meta Quest 3 headset** connected to your computer via Link or Air Link.

---

### Project Setup

1. **Clone the Repository**:  
   Use Git to clone the repository to your local machine:
   ```bash
   git clone https://github.com/minankk/MSc-AR-VR-Empathetic_Agent.git
2. **Open in Unity Hub**:

1. Open **Unity Hub** and click the "Add" button.
2. Navigate to the cloned repository folder and select it.
3. Unity will automatically detect the correct editor version and open the project.

3. **Configure XR Plug-in Management**:

1. In the **Unity Editor**, go to `Edit > Project Settings`.
2. Select **XR Plug-in Management** from the left sidebar.
3. Under the **Android** tab, click the `+` icon and add the **Oculus XR Plug-in**.

4. **Open the Main Scene**:

1. In the **Project** window (usually at the bottom of the editor), navigate to `Assets/Scenes`.
2. Double-click the **main scene** file to open it in the editor.

---

### Building and Running on the Quest 3

1. Connect your **Quest 3** to your computer via **USB-C** or wirelessly via **Air Link**.
2. In the **Unity Editor**, go to `File > Build Settings`.
3. Ensure the platform is set to **Android**.
4. Make sure your **Quest 3** is detected under the **Run Device** dropdown.
5. Click the **Build and Run** button to deploy the project to your headset.

---

## Data and Licensing

- **Agent Model**: The character model for the empathetic agent is a **custom asset** included within this repository.
- **Transcript Data**: The project's functionality is based on a set of **pre-scripted transcripts** with specific keywords. These transcripts are located in the `Assets/Scripts` folder of the repository.

---

### Licensing

This project is licensed under the **MIT License**. For more details, see the `LICENSE.md` file in the repository.
