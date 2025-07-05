---
description: General coding standards and best practices
globs: ["**/*.cs", "**/*.cshtml", "**/*.razor", "**/*.js", "**/*.css"]
alwaysApply: true
---

# General Development Rules

## Code Quality Standards

### Fundamental Principles
- Write clean, simple, readable code
- Implement features in the simplest possible way
- Keep files small and focused (<200 lines)
- Test after every meaningful change
- Focus on core functionality before optimization
- Use clear consistent naming
- Think thoroughly before coding. Write 2-3 reasoning paragraphs
- ALWAYS write simple, clean and modular code
- Use clear and easy-to-understand language. Write in short sentences

### Error Fixing
- DO NOT JUMP TO CONCLUSIONS! Consider multiple possible causes before deciding
- Explain the problem in plain English
- Make minimal necessary changes, changing as few lines of code as possible
- In case of strange errors, ask the user to perform a Perplexity web search to find the latest up-to-date information

### Building Process
- Verify each new feature works by telling the user how to test it
- DO NOT write complicated and confusing code. Opt for the simple & modular approach
- When not sure what to do, tell the user to perform a web search

### Comments
- ALWAYS try to add more helpful and explanatory comments into code
- NEVER delete old comments - unless they are obviously wrong or obsolete
- Include LOTS of explanatory comments in code. ALWAYS write well-documented code
- Document all changes and their reasoning IN THE COMMENTS
- When writing comments, use clear and easy-to-understand language. Write in short sentences

## Clean Code Principles

### Don't Repeat Yourself (DRY)
- Duplication of code can make code very difficult to maintain
- Any change in logic can make the code prone to bugs or make code changes difficult
- Create functions and classes to ensure logic is written in only one place
- Every piece of knowledge must have a single, unambiguous, authoritative representation within a system

### Curly's Law - Do One Thing
- A entity (class, function, variable) should mean one thing, and one thing only
- It should not mean one thing in one circumstance and carry a different value from a different domain some other time
- It should not mean two things at once
- It should mean One Thing and should mean it all of the time

### Keep It Simple Stupid (KISS)
- Most systems work best if they are kept simple rather than made complicated
- Simplicity should be a key goal in design
- Unnecessary complexity should be avoided
- Simple code benefits:
  - Less time to write
  - Less chances of bugs
  - Easier to understand, debug and modify
- Do the simplest thing that could possibly work

### Don't Make Me Think
- Code should be easy to read and understand without much thinking
- If it isn't then there is a prospect of simplification

### You Aren't Gonna Need It (YAGNI)
- Always implement things when you actually need them, never when you just foresee that you need them
- Even if you're totally sure you'll need a feature later on, don't implement it now
- Usually it turns out either:
  - You don't need it after all, or
  - What you actually need is quite different from what you foresaw needing earlier
- This doesn't mean you should avoid building flexibility into your code
- It means you shouldn't overengineer something based on what you think you might need later on

### Premature Optimization is the Root of All Evil
- Programmers waste enormous amounts of time thinking about, or worrying about, the speed of noncritical parts of their programs
- These attempts at efficiency actually have a strong negative impact when debugging and maintenance are considered
- We should forget about small efficiencies, say about 97% of the time: premature optimization is the root of all evil
- Yet we should not pass up our opportunities in that critical 3% 