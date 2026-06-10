allprojects {
    repositories {
        google()
        mavenCentral()
    }
}

val newBuildDir: Directory =
    rootProject.layout.buildDirectory
        .dir("../../build")
        .get()
rootProject.layout.buildDirectory.value(newBuildDir)

subprojects {
    val newSubprojectBuildDir: Directory = newBuildDir.dir(project.name)
    project.layout.buildDirectory.value(newSubprojectBuildDir)
}
subprojects {
    project.evaluationDependsOn(":app")
}

// Force all plugin subprojects to use compileSdk 36 to satisfy
// flutter_plugin_android_lifecycle requirement, even if older plugins
// (e.g. file_picker 8.x) hardcode a lower compileSdk.
// Skip :app (already evaluated via evaluationDependsOn).
subprojects {
    if (project.name != "app") {
        afterEvaluate {
            if (plugins.hasPlugin("com.android.library")) {
                extensions.configure<com.android.build.gradle.LibraryExtension> {
                    if (compileSdk != null && compileSdk!! < 36) {
                        compileSdk = 36
                    }
                }
            }
        }
    }
}

tasks.register<Delete>("clean") {
    delete(rootProject.layout.buildDirectory)
}
