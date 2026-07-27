# DO NOT MERGE — Code Review Assistant Fixture

This branch intentionally contains review findings and a failing unit test. It is
only for exercising the automated pull-request lifecycle. No real secret,
credential, production configuration, or deployment setting is included.

## Expected HIGH-RISK findings

- Authorization is bypassed because the supplied role is ignored.
- Employee input is concatenated into a SQL query string.
- Bonus percentage handling is ambiguous and can change financial results.

## Expected LOW-RISK findings

- Console logging is used instead of structured logging.
- Name formatting lacks defensive validation/null handling.
- Employee and manager formatting duplicate the same implementation.
- Log message naming/capitalization is inconsistent.

## Intentional CI failure

`IntentionalPipelineFailureTests` contains a deterministic failing assertion.
The existing pull-request test workflow will therefore remain red until the test
fixture is removed or corrected.

Delete this document and both fixture source files when the exercise is complete.
