SimpleOpenCodeCoverageFramework
===============================

# Usage
1. Modify Java source code files for measuring coverage.
  ```
  SimpleOccf.exe arg1 arg2 ...
    args: Java source code files or directories
  ```

2. Compile modified source code files linking [the runtime jar file](https://github.com/kohyatoh/soccf-runtime). You can use Maven dependency as follows.
  ```
  <dependency>
    <groupId>net.klazz.soccf</groupId>
    <artifactId>soccf-runtime</artifactId>
    <version>0.1</version>
    <optional>true</optional>
  </dependency>  
  ```

3. After executing test (e.g. `mvn test`), a logging file (`soccf.cov.gz`) is generated. You can analyze the logging file to compute coverage values. The format of the logging file is [here](https://github.com/kohyatoh/soccf-runtime/blob/master/java/src/main/java/net/klazz/soccf/runtime/CoverageCounter.java).
