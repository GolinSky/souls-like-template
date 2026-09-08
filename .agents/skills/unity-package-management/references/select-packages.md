# Package selection notes

Prefer an existing package that already satisfies the task. Choose a package by
the specific feature and this project's current Editor and render pipeline, not
by a generic genre template. Confirm the package id and compatible version from
the Package Manager or official Unity documentation before adding it.

For ProBuilder, inspect the installed state first. When absent, `com.unity.probuilder`
is the candidate package; evaluate the requested/official version against the
actual Unity Editor before changing the project. A package install does not
prove that its default materials are compatible with the active render pipeline.
