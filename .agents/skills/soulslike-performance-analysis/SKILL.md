---
name: soulslike-performance-analysis
description: Collect and interpret evidence for a bounded SoulsLike Unity performance scenario. Use for profiling, allocations, frame spikes, rendering, loading, or synchronization analysis before optimization.
---

# SoulsLike Performance Analysis

1. Define the reproduction scenario, platform, build or Editor conditions, and comparison baseline.
2. Collect only the measurements needed for that scenario.
3. Order findings by measured impact and distinguish Editor-only cost from player/runtime cost.
4. Connect evidence to likely owning code paths without presenting hypotheses as facts.
5. Use `$soulslike-context` only when a registered domain key materially affects the scenario.

Return conditions, measured findings, likely owners, unproven hypotheses, and the next smallest measurement. Do not implement optimizations.
