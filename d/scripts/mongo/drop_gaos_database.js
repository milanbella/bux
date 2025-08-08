(async () => {
  try {
    db = await connect('mongodb://localhost/gaos');
    console.log("INFO: Dropping mongo database 'gaos'");
    await db.dropDatabase();
  } catch (error) {
    console.error("ERROR: Could not drop the database 'gaos':", error.message);
    throw new Error("could not drop database 'gaos'");
  }
})();